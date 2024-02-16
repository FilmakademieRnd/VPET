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

class sceneCharacter:
    pass
        
class geoPackage:
    pass

class materialPackage:
    pass

class texturePackage:
    pass

class characterPackage:
    pass

def initialize():
    global vpet, v_prop
    vpet = bpy.context.window_manager.vpet_data
    v_prop = bpy.context.scene.vpet_properties

## General function to gather scene data
#
def gatherSceneData():
    initialize()
    static_objects = []
    editable_objects = []
     #cID
    vpet.cID = int(str(v_prop.server_ip).split('.')[3])
    # check if VPET collections exist
    if bpy.data.collections.find(v_prop.vpet_collection) > -1:
        static_objects = list(bpy.data.collections[v_prop.vpet_collection].all_objects)
        objectList = static_objects
        print(f'found {len(static_objects)} static objects')
    if bpy.data.collections.find(v_prop.edit_collection) > -1:    
        editable_objects = list(bpy.data.collections[v_prop.edit_collection].all_objects)
        objectList += editable_objects
        print(f'found {len(editable_objects)} editable objects')

    if len(objectList) > 0:
        # create empty as scene root node
        root = bpy.context.scene.objects.get('VPETsceneRoot')
        if root == None:    
            bpy.ops.object.empty_add(type='PLAIN_AXES', rotation=(0,0,0), location=(0, 0, 0), scale=(1, 1, 1))
            bpy.context.active_object.name = 'VPETsceneRoot'
            root = bpy.context.active_object
        else:
            bpy.context.view_layer.objects.active = root
            
        # sort scene objects by childcount
        objectList.sort(key=lambda x: len(x.children), reverse = True)
        
        # count toplevel objects bc they will be parented to sceneRoot
        for i, obj in enumerate(objectList):
            if type(obj.parent).__name__ == 'NoneType':
                vpet.rootChildCount += 1
        
        # add sceneRoot to list of objects to transfer
        objectList.insert(0, bpy.context.active_object)

        vpet.objectsToTransfer = objectList
        

        #iterate over all objects in the scene
        for i, n in enumerate(vpet.objectsToTransfer):
            processSceneObject(n, i)


        for i, n in enumerate(editable_objects):
            processEditableObjects(n, i)

        getHeaderByteArray()
        getNodesByteArray()
        getGeoBytesArray()
        getMaterialsByteArray()
        getTexturesByteArray()
        getCharacterByteArray()
        
        # delete Scene Root object - scene will remain unchanged
        bpy.ops.object.delete(use_global = False)

        for i, v in enumerate(vpet.nodeList):
            if v.editable == 1:
                vpet.editableList.append((bytearray(v.name).decode('ascii'), v.vpetType))
        
        return len(vpet.objectsToTransfer)-1
    
    else:
        return 0
    
    
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
        nodeMesh = sceneMesh()
        nodeMesh.vpetType = vpet.nodeTypes.index('GEO')
        nodeMesh.color = (obj.color[0], obj.color[1], obj.color[2], obj.color[3])
        nodeMesh.roughness = 0.5
        nodeMesh.materialId = -1
        
        # get geo data of mesh
        #nodeMesh.geoId = processGeometry(obj)
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
            
            #if nodeMaterial.tex != None:
                #nodeMesh.textureId = processTexture(nodeMaterial.tex)

        node = nodeMesh  

    
    elif obj.type == 'ARMATURE':
        nodeCharacter = sceneCharacter()
        nodeCharacter.vpetType = vpet.nodeTypes.index('SKINNEDMESH')
        nodeCharacter.color = (0,0,0,1)

    
 
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
    
    # get parent index
    parentId = -1
    for i, n in enumerate(vpet.objectsToTransfer):
        if n == obj.parent:
            parentId = i

    # check if node is editable
    if bpy.data.collections[v_prop.edit_collection] in obj.users_collection:
        node.editable = 1
    else:
        node.editable = 0
    
    # find parent in nodeList if there is one
    if parentId != -1:
        for i, n in enumerate(vpet.nodeList):
            # insert after parent into nodeList
            if n.vpetId == parentId:
                vpet.nodeList.insert(i+1, node)
    else:
        vpet.nodeList.append(node)

def processCharacter(armature_obj, object_list):
    chr_pack = characterPackage()
    chr_pack.bonePosition = [()]
    chr_pack.boneRotation = [()]
    chr_pack.boneScale = [()]

    if armature_obj.type == 'ARMATURE':
        bones = armature_obj.data.bones
        chr_pack.id = len(vpet.objectsToTransfer)

        chr_pack.bMSize = len(bones)
        chr_pack.boneMapping = [-1] * chr_pack.bMSize

        for i, bone in enumerate(bones):

            chr_pack.boneMapping[i] = object_list.index(bone.name) if bone.name in object_list else -1
            
            bone_matrix = armature_obj.matrix_world @ bone.matrix_local
            position = bone_matrix.to_translation()
            rotation = bone_matrix.to_quaternion()
            scale = bone_matrix.to_scale()

            chr_pack.bonePosition.extend([position.x, position.y, position.z])
            chr_pack.boneRotation.extend([rotation.x, rotation.y, rotation.z, rotation.w])
            chr_pack.boneScale.extend([scale.x, scale.y, scale.z])
    return chr_pack

def processEditableObjects(obj, index):

    if obj.type == "ARMATURE":
        processCharacter(obj, vpet.objectsToTransfer)

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




def PrintEvent(sender, new_value):
    print("Scenedist PrintEvent ")
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
        print(mesh.name)
        for a in links:
            print(a.from_node.image)
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

def get_vertex_bone_weights_and_indices(vertex, vertex_groups, bone_names):
    bone_weights = [0.0] * 4
    bone_indices = [-1] * 4

    weight_bone_pairs = []
    for group in vertex.groups:
        if group.group < len(vertex_groups) and vertex_groups[group.group].name in bone_names:
            bone_index = bone_names[vertex_groups[group.group].name]
            weight_bone_pairs.append((group.weight, bone_index))

    # Sort by weight in descending order and take the top 4
    weight_bone_pairs.sort(reverse=True, key=lambda x: x[0])
    weight_bone_pairs = weight_bone_pairs[:4]

    for i, pair in enumerate(weight_bone_pairs):
        bone_weights[i] = pair[0]
        bone_indices[i] = pair[1]

    return bone_weights, bone_indices

def processGeoNew(mesh):
    geoPack = sceneMesh()
    mesh_identifier = generate_mesh_identifier(mesh)

    for existing_geo in vpet.geoList:
        if existing_geo.identifier == mesh_identifier:
            return vpet.geoList.index(existing_geo)


    if mesh.parent.type == 'ARMATURE':
        isParentArmature = True
        armature = mesh.parent
        bone_names = {bone.name: idx for idx, bone in enumerate(armature.data.bones)}
        vertex_bone_weights = {}
        vertex_bone_indices = {}

        for vert in mesh.data.vertices:
            weights, indices = get_vertex_bone_weights_and_indices(vert, mesh.vertex_groups, bone_names)
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
        print("BWSIZE IS: " + str(geoPack.bWSize))
        print("cobuffer size IS: " + str(len(co_buffer)))
        print("BONE WEIGHTS SIZE IS: " + str(len(geoPack.boneWeights)))

    for i, vert in enumerate(interleaved_buffer):
        geoPack.vertices.append(vert[0][0])
        geoPack.vertices.append(vert[0][2])
        geoPack.vertices.append(vert[0][1])

        geoPack.normals.append(-vert[1][0])
        geoPack.normals.append(-vert[1][2])
        geoPack.normals.append(-vert[1][1])
        
        geoPack.uvs.append(vert[2][0])
        geoPack.uvs.append(vert[2][1])

        #geoPack.indices.append(i)
    print("geopack Vertices size is: " + str(len(geoPack.vertices)))
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
        charBinary = bytearray([]) 

            


            
            