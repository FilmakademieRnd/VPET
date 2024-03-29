import functools
import math
from ..AbstractParameter import Parameter;
from ..serverAdapter import SendParameterUpdate;



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
        self._parameterList.append(position)
        rotation = Parameter(obj.rotation_quaternion, "Rotation", self)
        self._parameterList.append(rotation)
        scale = Parameter(obj.scale, "Scale", self)
        self._parameterList.append(scale)
        # Bind UpdatePosition to the instance using functools.partial
        position.hasChanged.append(functools.partial(self.UpdatePosition, position))
        rotation.hasChanged.append(functools.partial(self.Updaterotation, rotation))
        scale.hasChanged.append(functools.partial(self.UpdateScale, scale))


    def UpdatePosition(self, parameter, new_value):
        if self._lock == True:
            self.editableObject.location = new_value
        else:
            SendParameterUpdate(parameter)

    def Updaterotation(self, parameter, new_value):
        if self._lock == True:
            self.editableObject.rotation_mode = 'QUATERNION'
            self.editableObject.rotation_quaternion = new_value
            self.editableObject.rotation_mode = 'XYZ'

            if self.editableObject.type == 'LIGHT' or self.editableObject.type == 'CAMERA':
                self.editableObject.rotation_euler.rotate_axis("X", math.radians(90))
        else:
            SendParameterUpdate(parameter)

    def UpdateScale(self, parameter, new_value):
        if self._lock == True:
            self.editableObject.scale = new_value
        else:
            SendParameterUpdate(parameter)

    def LockUnlock(self, value):
        if value == 1:
            self._lock = True
            self.editableObject.hide_select = True
        else:
            self._lock = False
            self.editableObject.hide_select = False
        
    
