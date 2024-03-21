"""
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tools
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET component Blender Scene Distribution is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the Blender Scene Distribution is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the Blender Scene Distribution Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Blender Scene Distribution in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
"""

import bpy
import math
import mathutils
import bmesh
import struct

from .AbstractParameter import Parameter
from .SceneObjects.SceneObject import SceneObject
from .SceneObjects.SceneObjectCamera import SceneObjectCamera
from .SceneObjects.SceneObjectLight import SceneObjectLight
from .SceneObjects.SceneObjectSpotLight import SceneObjectSpotLight
from .SceneObjects.SceneCharacterObject import SceneCharacterObject
from .Avatar_HumanDescriptioon_Mixamo import blender_to_unity_bone_mapping


## Creating empty classes to store node data
#  is there a more elegant way?
class sceneObject:
    pass

class sceneLight:
    pass
        
class sceneCamera:
    pass
        
class sceneMesh:
    pass

class sceneSkinnedmesh:
    pass
        
class geoPackage:
    pass

class materialPackage:
    pass

class texturePackage:
    pass

class characterPackage:
    pass

class curvePackage:
    pass

def initialize():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties

## General function to gather scene data
#
def gatherSceneData():
    initialize()
     #cID
    vpet.cID = int(str(v_prop.server_ip).split('.')[3])
    objectList = getObjectList()

    if len(objectList) > 0:
        vpet.objectsToTransfer = objectList
        

        #iterate over all objects in the scene
        for i, n in enumerate(vpet.objectsToTransfer):
            processSceneObject(n, i)

        for i, n in enumerate(vpet.objectsToTransfer):
            processEditableObjects(n, i)

        getHeaderByteArray()
        getNodesByteArray()
        getGeoBytesArray()
        getMaterialsByteArray()
        getTexturesByteArray()
        getCharacterByteArray()
        
        # delete Scene Root object - scene will remain unchanged
        #bpy.ops.object.delete(use_global = False)

        for i, v in enumerate(vpet.nodeList):
            if v.editable == 1:
                vpet.editableList.append((bytearray(v.name).decode('ascii'), v.vpetType))
        
        return len(vpet.objectsToTransfer)
    
    else:
        return 0
    

def getObjectList():
    parent_object_name = "VPETsceneRoot"
    parent_object = bpy.data.objects.get(parent_object_name)
    objectList = []
    # TODO OBJ NR 
    recursive_game_object_id_extract(parent_object, objectList)

    
    return objectList    

def recursive_game_object_id_extract(location, objectList):
    # Iterate through each child of the location
    for child in location.children:
        # Add the child object to the game_objects list
        print(child.name)
        objectList.append(child)
        # Recursively call the function for the child to explore its children
        recursive_game_object_id_extract(child, objectList)    
    
## Process and store a scene object
#
# @param obj The scene object to process
# @param index The objects index in the list of all objects
def processSceneObject(obj, index):
    global vpet, v_prop
    node = sceneObject()
    node.vpetType = vpet.nodeTypes.index('GROUP')
    
    # gather light data
    if obj.type == 'LIGHT':
        nodeLight = sceneLight()
        nodeLight.vpetType =vpet.nodeTypes.index('LIGHT')
        nodeLight.lightType = vpet.lightTypes.index(obj.data.type)
        nodeLight.intensity = obj.data.energy/100
        nodeLight.color = (obj.data.color.r, obj.data.color.g, obj.data.color.b)
        nodeLight.type = obj.data.type
        # placeholder value bc Blender does not use exposure
        nodeLight.exposure = 0
        # placeholder value bc Blender has no range
        nodeLight.range = 10
        if obj.data.type == 'SPOT':
            nodeLight.angle = math.degrees(obj.data.spot_size)
        else:
            nodeLight.angle = 45

        node = nodeLight
    
    # gather camera data    
    elif obj.type == 'CAMERA':
        nodeCamera = sceneCamera()
        nodeCamera.vpetType = vpet.nodeTypes.index('CAMERA')
        nodeCamera.fov = math.degrees(obj.data.angle)
        nodeCamera.aspect = obj.data.sensor_width/obj.data.sensor_height
        nodeCamera.near = obj.data.clip_start
        nodeCamera.far = obj.data.clip_end  
        nodeCamera.focalDist = 5
        nodeCamera.aperture = 2    
        node = nodeCamera
    
    # gather mesh data
    elif obj.type == 'MESH':
        if obj.parent != None:
            if obj.parent.type == 'ARMATURE':
                nodeSkinMesh = sceneSkinnedmesh()
                node = processSkinnedMesh(obj, nodeSkinMesh)
            else:
                nodeMesh = sceneMesh()
                node = processMesh(obj, nodeMesh)      
        else:
            nodeMesh = sceneMesh()
            node = processMesh(obj, nodeMesh)
                
    

    elif obj.type == 'ARMATURE':
        node.vpetType = vpet.nodeTypes.index('CHARACTER')
        processCharacter(obj, vpet.objectsToTransfer)

    elif obj.type == 'CURVE':
        processCurve(obj, vpet.objectsToTransfer)
        
    # gather general node data    
    nodeMatrix = obj.matrix_local.copy()

    node.position = (nodeMatrix.to_translation().x, nodeMatrix.to_translation().z, nodeMatrix.to_translation().y)
    node.scale = (nodeMatrix.to_scale().x, nodeMatrix.to_scale().z, nodeMatrix.to_scale().y)
    
    # camera and light rotation offset
    if obj.type == 'CAMERA' or obj.type == 'LIGHT':
        rotFix = mathutils.Matrix.Rotation(math.radians(-90.0), 4, 'X')
        nodeMatrix = nodeMatrix @ rotFix
    
    rot = nodeMatrix.to_quaternion()
    rot.invert()
    node.rotation = (rot[1], rot[3], rot[2], rot[0])
    
    node.name = bytearray(64)
    
    for i, n in enumerate(obj.name.encode()):
        node.name[i] = n
    node.childCount = len(obj.children)
    
    
    if obj.name == 'VPETsceneRoot':
        node.childCount = vpet.rootChildCount
        
    node.vpetId = index

    edit_property = "VPET-Editable"
    # check if node is editable
    if edit_property in obj and obj.get(edit_property):
        node.editable = 1
        vpet.editable_objects.append(obj)
    else:
        node.editable = 0

    if obj.name != 'VPETsceneRoot':
        vpet.nodeList.append(node)
    
def processMesh(obj, nodeMesh): 
    nodeMesh.vpetType = vpet.nodeTypes.index('GEO')
    nodeMesh.color = (obj.color[0], obj.color[1], obj.color[2], obj.color[3])
    nodeMesh.roughness = 0.5
    nodeMesh.materialId = -1
               
    # get geo data of mesh
    nodeMesh.geoId = processGeoNew(obj)
                
    # get material of mesh
    nodeMaterial = materialPackage()
    mat = obj.active_material
                
    if mat != None:
        nodeMesh.materialId = processMaterial(obj)
        nodeMaterial = vpet.materialList[nodeMesh.materialId]
                    
        # add material parameters to node
        nodeMesh.color = nodeMaterial.color
        nodeMesh.roughness = nodeMaterial.roughness
        nodeMesh.specular = nodeMaterial.specular
                    
    return(nodeMesh)

def processSkinnedMesh(obj, nodeSkinMesh):
    nodeSkinMesh.vpetType = vpet.nodeTypes.index('SKINNEDMESH')
    nodeSkinMesh.color = (0,0,0,1)
    nodeSkinMesh.roughness = 0.5
    nodeSkinMesh.materialId = -1
    nodeSkinMesh.characterRootID = vpet.objectsToTransfer.index(obj.parent)
    
    nodeSkinMesh.geoID = processGeoNew(obj)
    # get material of mesh
    nodeMaterial = materialPackage()
    mat = obj.active_material
    if mat != None:
        nodeSkinMesh.materialId = processMaterial(obj)
        nodeMaterial = vpet.materialList[nodeSkinMesh.materialId]
        
        # add material parameters to node
        nodeSkinMesh.color = nodeMaterial.color
        nodeSkinMesh.roughness = nodeMaterial.roughness
        nodeSkinMesh.specular = nodeMaterial.specular

        bbox_corners = [obj.parent.matrix_world @ mathutils.Vector(corner) for corner in obj.parent.bound_box]
        bbox_center = sum(bbox_corners, mathutils.Vector((0,0,0))) / 8
        bbox_extents = max(bbox_corners, key=lambda c: (c - bbox_center).length) - bbox_center
        nodeSkinMesh.boundCenter = [bbox_center.x, bbox_center.z, bbox_center.y]
        nodeSkinMesh.boundExtents = [bbox_extents.x, bbox_extents.z, bbox_extents.y]

        armature_obj = obj.parent
        if armature_obj:
            armature_data = armature_obj.data
            bind_poses = []
            for bone in armature_data.bones:
                bind_matrix = armature_obj.matrix_world @ bone.matrix_local
                for row in bind_matrix:
                    bind_poses.extend(row)
            
            desired_length = 1584
            current_length = len(bind_poses)
            if current_length < desired_length:
                # If bind_poses is shorter, extend with zeroes
                bind_poses.extend([0] * (desired_length - current_length))  
            nodeSkinMesh.bindPoses = bind_poses
            nodeSkinMesh.bindPoseLength = int(len(bind_poses) / 16)
            nodeSkinMesh.skinnedMeshBoneIDs = [-1] * 99  # Initialize all to -1
            for i, bone in enumerate(armature_data.bones):
                bone_index = -1
                for idx, obj in enumerate(vpet.objectsToTransfer):
                    if obj.name == bone.name:
                        bone_index = idx
                        break
            #for i, bone in enumerate(armature_data.bones):  
                nodeSkinMesh.skinnedMeshBoneIDs[i] = bone_index
                

        nodeSkinMesh.skinnedMeshBoneIDsSize = len(nodeSkinMesh.skinnedMeshBoneIDs)        

        return(nodeSkinMesh)



def processCharacter(armature_obj, object_list):
    chr_pack = characterPackage()
    chr_pack.bonePosition = []
    chr_pack.boneRotation = []
    chr_pack.boneScale = []
    chr_pack.boneMapping = []
    chr_pack.skeletonMapping = []
        

    if armature_obj.type == 'ARMATURE':
        bones = armature_obj.data.bones
        chr_pack.characterRootID = vpet.objectsToTransfer.index(armature_obj)

        if(v_prop.mixamo_humanoid):
            for key, value in blender_to_unity_bone_mapping.items():
                bone_index = -1
                for idx, obj in enumerate(object_list):
                    if key == obj.name:
                        bone_index = idx
                        break
                chr_pack.boneMapping.append(bone_index)

        else:
            for i, bone in enumerate(bones):
                bone_index = -1
                for idx, obj in enumerate(vpet.objectsToTransfer):
                    if obj.name == bone.name:
                        bone_index = idx
                        break

                chr_pack.boneMapping.append(bone_index)
        
        chr_pack.bMSize = len(chr_pack.boneMapping)
        
        for idx, obj in enumerate(object_list):
                if obj.name == armature_obj.name:
                    bone_index = idx
        chr_pack.skeletonMapping.append(bone_index)

        nodeMatrix = armature_obj.matrix_local.copy()

        chr_pack.bonePosition.extend([nodeMatrix.to_translation().x, nodeMatrix.to_translation().z, nodeMatrix.to_translation().y])
        chr_pack.boneScale.extend([nodeMatrix.to_scale().x, nodeMatrix.to_scale().z, nodeMatrix.to_scale().y])
        rot = nodeMatrix.to_quaternion()
        rot.invert()
        chr_pack.boneRotation.extend([rot[1], rot[3], rot[2], rot[0]])

        for mesh in armature_obj.children:
            if mesh.type == 'MESH':
                for idx, obj in enumerate(object_list):
                    if obj.name == mesh.name:
                        bone_index = idx
                chr_pack.skeletonMapping.append(bone_index)

                nodeMatrix = mesh.matrix_local.copy()

                chr_pack.bonePosition.extend([nodeMatrix.to_translation().x, nodeMatrix.to_translation().z, nodeMatrix.to_translation().y])
                chr_pack.boneScale.extend([nodeMatrix.to_scale().x, nodeMatrix.to_scale().z, nodeMatrix.to_scale().y])
                rot = nodeMatrix.to_quaternion()
                rot.invert()
                chr_pack.boneRotation.extend([rot[1], rot[3], rot[2], rot[0]])


        for bone in armature_obj.pose.bones:
            bone_index = -1
            for idx, obj in enumerate(object_list):
                if obj.name == bone.name:
                    bone_index = idx
                    break

            chr_pack.skeletonMapping.append(bone_index)

            
            bone_matrix = armature_obj.matrix_world @ bone.matrix 
            position = bone_matrix.to_translation()
            rotation = bone_matrix.to_quaternion()
            rotation.invert()
            
            
            scale =bone.scale
            chr_pack.bonePosition.extend([position.x, position.z, position.y])
            chr_pack.boneRotation.extend([rotation[1], rotation[3], rotation[2], rotation[0]])
            chr_pack.boneScale.extend(scale)

        chr_pack.sMSize = len(chr_pack.skeletonMapping)
    vpet.characterList.append(chr_pack)
    return chr_pack

def processCurve(obj, objList):
    curve_Pack = curvePackage()

    for frame in range(bpy.context.scene.frame_start, bpy.context.scene.frame_end + 1):
        points = evaluate_curve(obj, frame)
        vpet.points_for_frames[frame] = points


    curve_Pack.points = list(vpet.points_for_frames.values())
    curve_Pack.pointsLen = len(curve_Pack.points) # len is also equal to the nr of frames 

    vpet.curveList.append(curve_Pack)


def evaluate_curve(curve_object, frame):
    # Set the current frame
    bpy.context.scene.frame_set(frame)
    
    evaluated_points = []
    
    # Ensure the object is a curve and is using Bezier type
    if curve_object.data.splines.active.type == 'BEZIER':
        spline = curve_object.data.splines.active
        
        # Evaluate the curve at current frame
        evaluated_point = spline.bezier_points[0].co.lerp(spline.bezier_points[1].co, frame / bpy.context.scene.frame_end)
        evaluated_points.append(evaluated_point)
    
    return evaluated_points

def processEditableObjects(obj, index):

    #if obj.type == "ARMATURE":
     #   processCharacter(obj, vpet.objectsToTransfer)

    if obj.type == 'MESH':
        aaa = SceneObject(obj)
        vpet.SceneObjects.append(aaa)
    elif obj.type == 'CAMERA':
        aaa = SceneObjectCamera(obj)
        vpet.SceneObjects.append(aaa)
    elif obj.type == 'LIGHT':
        if obj.data.type == 'SPOT':
            aaa = SceneObjectSpotLight(obj)
            vpet.SceneObjects.append(aaa)
        else:
            aaa = SceneObjectLight(obj)
            vpet.SceneObjects.append(aaa)
    elif obj.type == 'ARMATURE':

        aaa = SceneCharacterObject(obj)
        vpet.SceneObjects.append(aaa)

## Process a meshes material
#
# @param mesh The geo data to process
#
#  this breaks easily if the material has a complex setup
#  todo:
#  - should find a more stable way to traverse the shader node graph
#  - should maybe skip the whole object if it has a volume shader
def processMaterial(mesh):
    matPack = materialPackage()
    
    mat = mesh.active_material
    
    # get material data
    name = mesh.active_material.name
    matPack.type = 1
    src = "Standard" 
    matPack.textureId = -1
    
    # need to check if the material was already processed
    for i, n in enumerate(vpet.materialList):
        if n.name == name:
            return i
        

    matPack.name = bytearray(64)
    matPack.src = bytearray(64)
    
    for i, n in enumerate(mesh.active_material.name.encode()):
        matPack.name[i] = n
 

    for i, n in enumerate(src.encode()):
        matPack.src[i] = n    
    
    # getting the material data
    matPack.color = mesh.active_material.diffuse_color
    matPack.roughness = mesh.active_material.roughness
    matPack.specular = mesh.active_material.specular_intensity
    
    ## get into the node tree
    # find output of node tree
    out = None
    for n in (x for x in mat.node_tree.nodes if x.type == 'OUTPUT_MATERIAL'):
        out = n
        break

    # the node connected to the first input of the OUT should always be a shader
    shader = out.inputs[0].links[0].from_node
    
    if shader != None:
        tmpColor = shader.inputs[0].default_value
        matPack.color = (tmpColor[0], tmpColor[1], tmpColor[2], tmpColor[3])
        
        if shader.type == 'BSDF_PRINCIPLED':
            matPack.roughness = shader.inputs[7].default_value
            matPack.specular = shader.inputs[5].default_value
        
    # check if texture is plugged in
    matPack.tex = None
    links = shader.inputs[0].links
    if len(links) > 0:
        if links[0].from_node.type == 'TEX_IMAGE':
            matPack.tex = links[0].from_node.image
            

    if matPack.tex != None:
        matPack.textureId = processTexture(matPack.tex)
    
            
    matPack.diffuseTexture = matPack.tex
    matPack.materialID = len(vpet.materialList)
    vpet.materialList.append(matPack)
    return (len(vpet.materialList)-1)
    
## Process Texture
#
# @param tex Texture to process
def processTexture(tex):
    # check if texture is already processed
    for i, t in enumerate(vpet.textureList):
        if t.texture == tex.name_full:
            return i

    try:
        texFile = open(tex.filepath_from_user(), 'rb')
    except FileNotFoundError:
        print(f"Error: Texture file not found at {tex.filepath_from_user()}")
        return -1
    
    texBytes = texFile.read()
    
    texPack = texturePackage()
    texPack.colorMapData = texBytes
    texPack.colorMapDataSize = len(texBytes)
    texPack.width = tex.size[0]
    texPack.height = tex.size[1]
    texPack.format = 0
    
    texPack.texture = tex.name_full
    
    texFile.close()

    texBinary = bytearray([])
        
    #texBinary.extend(struct.pack('i', 0)) #type
    texBinary.extend(struct.pack('i', texPack.width))
    texBinary.extend(struct.pack('i', texPack.height))
    texBinary.extend(struct.pack('i', texPack.format))
    texBinary.extend(struct.pack('i', texPack.colorMapDataSize))
    texBinary.extend(texPack.colorMapData)
    
    vpet.textureList.append(texPack)
    
    # return index of texture in texture list
    return (len(vpet.textureList)-1)

def get_vertex_bone_weights_and_indices(vert):
    #for vert_idx, vert in enumerate(obj.data.vertices):
        # Retrieve the vertex groups and their weights for this vertex
        groups = [(g.group, g.weight) for g in vert.groups]
        
        # Sort the groups by weight in descending order
        groups.sort(key=lambda x: x[1], reverse=True)
        
        # Limit to at most 4 bone influences
        groups = groups[:4]
        while len(groups) < 4:
            groups.append((0, 0.0))
        
        # Output the bone indices and weights for this vertex
        bone_indices = [g[0] for g in groups]
        bone_weights = [g[1] for g in groups]
        
        return bone_weights, bone_indices

def processGeoNew(mesh):
    geoPack = sceneMesh()
    mesh_identifier = generate_mesh_identifier(mesh)
    vertex_bone_weights = {}
    vertex_bone_indices = {}
    isParentArmature = False

    for existing_geo in vpet.geoList:
        if existing_geo.identifier == mesh_identifier:
            return vpet.geoList.index(existing_geo)

    if mesh.parent != None:
        if mesh.parent.type == 'ARMATURE':
            isParentArmature = True
            armature = mesh.parent
            bone_names = {bone.name: idx for idx, bone in enumerate(armature.data.bones)}
            

            for vert in mesh.data.vertices:
                weights, indices = get_vertex_bone_weights_and_indices(vert)
                vertex_bone_weights[vert.index] = weights
                vertex_bone_indices[vert.index] = indices

    mesh.data.calc_normals_split()
    bm = bmesh.new()
    bm.from_mesh(mesh.data)

    # flipping faces because the following axis swap inverts them
    for f in bm.faces:
        bmesh.utils.face_flip(f)
    bm.normal_update()

    bm.verts.ensure_lookup_table()
    uv_layer = bm.loops.layers.uv.active
    loop_triangles = bm.calc_loop_triangles()

    split_verts = {} # vertex data : some unique counted index using hash map for quick lookup
    index_buffer = []
    split_index_cur = 0 # index of vert after which the hash_map can later be sorted into a list again
    num_shared_verts = 0 # just for debugging purposes
    for tri in loop_triangles:
        for loop in tri:
            original_index = loop.vert.index
            co = loop.vert.co.copy().freeze()
            uv = loop[uv_layer].uv.copy().freeze()

            if mesh.data.polygons[0].use_smooth:
                normal = loop.vert.normal.copy().freeze() if loop.edge.smooth else loop.face.normal.copy().freeze()
            else:
                normal = loop.face.normal.copy().freeze()
            
            bone_weights = [0.0] * 4
            bone_indices = [-1] * 4
            if mesh.parent != None:
                if mesh.parent.type == 'ARMATURE':
                    bone_weights = vertex_bone_weights.get(original_index, [0.0] * 4)
                    bone_indices = vertex_bone_indices.get(original_index, [-1] * 4)

            #normal = loop.vert.normal.copy().freeze() if loop.edge.smooth else loop.face.normal.copy().freeze()
            new_split_vert = (co, normal, uv, tuple(bone_weights), tuple(bone_indices))
            split_vert_idx = split_verts.get(new_split_vert)
            if split_vert_idx == None: # no matching vert found, push new one with index and increment for next time
                split_vert_idx = split_index_cur
                split_verts[new_split_vert] = split_vert_idx
                split_index_cur += 1
            else:
                num_shared_verts += 1
            index_buffer.append(split_vert_idx)

    split_vert_items = list(split_verts.items())
    split_vert_items.sort(key=lambda x: x[1]) #sort by index
    interleaved_buffer = [item[0] for item in split_vert_items] # strip off index
    co_buffer, normal_buffer, uv_buffer, bone_weights_buffer, bone_indices_buffer = zip(*interleaved_buffer)


    # should unify the list sizes
    geoPack.vSize = len(co_buffer)
    geoPack.iSize = len(index_buffer)
    geoPack.nSize = len(normal_buffer)
    geoPack.uvSize = len(uv_buffer)
    geoPack.bWSize = 0
    geoPack.vertices = []
    geoPack.indices = []
    geoPack.normals = []
    geoPack.uvs = []
    geoPack.boneWeights = []
    geoPack.boneIndices = []

    if isParentArmature:
        for vert_data in interleaved_buffer:
            _, _, _, vert_bone_weights, vert_bone_indices = vert_data
            geoPack.boneWeights.extend(vert_bone_weights)
            geoPack.boneIndices.extend(vert_bone_indices)

        geoPack.bWSize = len(co_buffer)

    for i, vert in enumerate(interleaved_buffer):
        geoPack.vertices.append(vert[0][0])
        geoPack.vertices.append(vert[0][2])
        geoPack.vertices.append(vert[0][1])

        geoPack.normals.append(-vert[1][0])
        geoPack.normals.append(-vert[1][2])
        geoPack.normals.append(-vert[1][1])
        
        geoPack.uvs.append(vert[2][0])
        geoPack.uvs.append(vert[2][1])
    bm.free()

    
    geoPack.indices = index_buffer
    geoPack.mesh = mesh
    geoPack.identifier = mesh_identifier
    
    vpet.geoList.append(geoPack)
    return (len(vpet.geoList)-1)

def generate_mesh_identifier(obj):
    if obj.type == 'MESH':
        return f"Mesh_{obj.name}_{len(obj.data.vertices)}"
    elif obj.type == 'ARMATURE':
        return f"Armature_{obj.name}_{len(obj.data.bones)}"
    else:
        return f"{obj.type}_{obj.name}"

## generate Byte Arrays out of collected node data
def getHeaderByteArray():
    global headerByteData
    headerBin = bytearray([])
    
    lightIntensityFactor = 1.0
    senderID = int(vpet.cID)

    headerBin.extend(struct.pack('f', lightIntensityFactor))
    headerBin.extend(struct.pack('i', senderID))
    headerBin.extend(struct.pack('i', 60))# frame rate that should be modified later

    vpet.headerByteData.extend(headerBin)

def getNodesByteArray():
    for node in vpet.nodeList:
        nodeBinary = bytearray([])
        
        nodeBinary.extend(struct.pack('i', node.vpetType))
        nodeBinary.extend(struct.pack('i', node.editable)) #editable ?
        nodeBinary.extend(struct.pack('i', node.childCount))
        nodeBinary.extend(struct.pack('3f', *node.position))
        nodeBinary.extend(struct.pack('3f', *node.scale))
        nodeBinary.extend(struct.pack('4f', *node.rotation))
        nodeBinary.extend(node.name)
          
        if (node.vpetType == vpet.nodeTypes.index('GEO')):
            nodeBinary.extend(struct.pack('i', node.geoId))
            nodeBinary.extend(struct.pack('i', node.materialId))
            nodeBinary.extend(struct.pack('4f', *node.color))
            
        if (node.vpetType == vpet.nodeTypes.index('LIGHT')):
            nodeBinary.extend(struct.pack('i', node.lightType))
            nodeBinary.extend(struct.pack('f', node.intensity))
            nodeBinary.extend(struct.pack('f', node.angle))
            nodeBinary.extend(struct.pack('f', node.range))
            nodeBinary.extend(struct.pack('3f', *node.color))
            
        if (node.vpetType == vpet.nodeTypes.index('CAMERA')):
            nodeBinary.extend(struct.pack('f', node.fov))
            nodeBinary.extend(struct.pack('f', node.aspect))
            nodeBinary.extend(struct.pack('f', node.near))
            nodeBinary.extend(struct.pack('f', node.far))
            nodeBinary.extend(struct.pack('f', node.focalDist))
            nodeBinary.extend(struct.pack('f', node.aperture))
        
        if (node.vpetType == vpet.nodeTypes.index('SKINNEDMESH')):
            nodeBinary.extend(struct.pack('i', node.geoID))
            nodeBinary.extend(struct.pack('i', node.materialId))
            nodeBinary.extend(struct.pack('4f', *node.color))
            nodeBinary.extend(struct.pack('i', node.bindPoseLength))
            nodeBinary.extend(struct.pack('i', node.characterRootID))
            nodeBinary.extend(struct.pack('3f', *node.boundExtents))
            nodeBinary.extend(struct.pack('3f', *node.boundCenter))
            nodeBinary.extend(struct.pack('%sf'% node.bindPoseLength * 16, *node.bindPoses))
            nodeBinary.extend(struct.pack('%si'% node.skinnedMeshBoneIDsSize, *node.skinnedMeshBoneIDs))
        
                    
        vpet.nodesByteData.extend(nodeBinary)

## pack geo data into byte array
def getGeoBytesArray():        
    for geo in vpet.geoList:
        geoBinary = bytearray([])
        
        geoBinary.extend(struct.pack('i', geo.vSize))
        geoBinary.extend(struct.pack('%sf' % geo.vSize*3, *geo.vertices))
        geoBinary.extend(struct.pack('i', geo.iSize))
        geoBinary.extend(struct.pack('%si' % geo.iSize, *geo.indices))
        geoBinary.extend(struct.pack('i', geo.nSize))
        geoBinary.extend(struct.pack('%sf' % geo.nSize*3, *geo.normals))
        geoBinary.extend(struct.pack('i', geo.uvSize))
        geoBinary.extend(struct.pack('%sf' % geo.uvSize*2, *geo.uvs))
        geoBinary.extend(struct.pack('i', geo.bWSize))
        if(geo.bWSize > 0):
            geoBinary.extend(struct.pack('%sf' % geo.bWSize*4, *geo.boneWeights))
            geoBinary.extend(struct.pack('%si' % geo.bWSize*4, *geo.boneIndices))

        
        vpet.geoByteData.extend(geoBinary)

## pack texture data into byte array        
def getTexturesByteArray():
    if len(vpet.textureList) > 0:
        for tex in vpet.textureList:
            texBinary = bytearray([])
    
            texBinary.extend(struct.pack('i', tex.width))
            texBinary.extend(struct.pack('i', tex.height))
            texBinary.extend(struct.pack('i', tex.format))
            texBinary.extend(struct.pack('i', tex.colorMapDataSize))
            texBinary.extend(tex.colorMapData)
            
            vpet.texturesByteData.extend(texBinary)

## pack Material data into byte array        
def getMaterialsByteArray():
    if len(vpet.materialList) > 0:
        for mat in vpet.materialList:
            matBinary = bytearray([])
            matBinary.extend(struct.pack('i', mat.type)) #type
            matBinary.extend(struct.pack('i', 64))# name.size
            matBinary.extend(mat.name) # mat name
            matBinary.extend(struct.pack('i', 64)) # src.size
            matBinary.extend(mat.src) # src
            matBinary.extend(struct.pack('i', mat.materialID)) # mat id
            matBinary.extend(struct.pack('i', len(vpet.textureList)))# tex id size
            if(mat.textureId != -1):
                matBinary.extend(struct.pack('i', mat.textureId))# tex id
                matBinary.extend(struct.pack('f', 0)) # tex offsets
                matBinary.extend(struct.pack('f', 0)) # tex offsets
                matBinary.extend(struct.pack('f', 1)) # tex scales
                matBinary.extend(struct.pack('f', 1)) # tex scales

            vpet.materialsByteData.extend(matBinary) 

def getCharacterByteArray():
    if len(vpet.characterList):
        for chr in vpet.characterList:
            charBinary = bytearray([]) 
            charBinary.extend(struct.pack('i', chr.bMSize))
            charBinary.extend(struct.pack('i', chr.sMSize)) 
            charBinary.extend(struct.pack('i', chr.characterRootID))
            formatBoneMAping = f'{len(chr.boneMapping)}i'
            formatSkeletonMAping = f'{len(chr.skeletonMapping)}i'
            charBinary.extend(struct.pack(formatBoneMAping, *chr.boneMapping))
            charBinary.extend(struct.pack(formatSkeletonMAping, *chr.skeletonMapping))
            #TODO WTF IS GOING ON
            charBinary.extend(struct.pack('%sf' % chr.sMSize*3, *chr.bonePosition))
            charBinary.extend(struct.pack('%sf' % chr.sMSize*4, *chr.boneRotation))
            charBinary.extend(struct.pack('%sf' % chr.sMSize*3, *chr.boneScale))

            vpet.charactersByteData.extend(charBinary) 

           
def getCurveByteArray():
    for curve in vpet.characterList:
        curveBinary = bytearray([])
        curveBinary.extend(struct.pack('i', curve.pointsLen))
        curveBinary.extend(struct.pack('%sf' % chr.sMSize*3, *curve.points))
