import functools
from ..AbstractParameter import Parameter;



class SceneObject:

    s_id = 1
    

    def __init__(self, obj):
        self._id = SceneObject.s_id
        SceneObject.s_id += 1
        self._sceneID = 254
        self._parameterList = []
        self._lock = False
        self.editableObject = obj 
        position = Parameter(obj.location, "Position", self)
        rotation = Parameter(obj.rotation_quaternion, "Rotation", self)
        scale = Parameter(obj.scale, "Scale", self)
        self._parameterList.append(position)
        self._parameterList.append(rotation)
        self._parameterList.append(scale)
        # Bind UpdatePosition to the instance using functools.partial
        position.hasChanged.append(functools.partial(self.UpdatePosition, position))
        rotation.hasChanged.append(functools.partial(self.Updaterotation, rotation))
        scale.hasChanged.append(functools.partial(self.UpdateScale, scale))




    def UpdatePosition(self, parameter, new_value):
        self.editableObject.location = new_value

    def Updaterotation(self, parameter, new_value):
        self.editableObject.rotation_mode = 'QUATERNION'
        self.editableObject.rotation_quaternion = new_value
        self.editableObject.rotation_mode = 'XYZ'

    def UpdateScale(self, parameter, new_value):
        self.editableObject.scale = new_value
        
    
