import functools
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject
from ..serverAdapter import SendParameterUpdate;


class SceneObjectCamera(SceneObject):
    def __init__(self, obj):
        super().__init__(obj)
        fov = Parameter(obj.data.angle, "Fov", self)
        self._parameterList.append(fov)
        aspect = Parameter(obj.data.sensor_width/obj.data.sensor_height, "Aspect", self)
        self._parameterList.append(aspect)
        near = Parameter(obj.data.clip_start, "Near", self)
        self._parameterList.append(near)
        far = Parameter(obj.data.clip_end, "Far", self)
        self._parameterList.append(far)

        fov.hasChanged.append(functools.partial(self.UpdateFov, fov))
        near.hasChanged.append(functools.partial(self.UpdateNear, near))
        far.hasChanged.append(functools.partial(self.UpdateFar, far))




    def UpdateFov(self, parameter, new_value):
         if self._lock == True:
            self.editableObject.data.angle = new_value
         else:
            SendParameterUpdate(parameter)

    def UpdateNear(self, parameter, new_value):
         if self._lock == True:
            self.editableObject.data.clip_start = new_value
         else:
            SendParameterUpdate(parameter)

    def UpdateFar(self, parameter, new_value):
         if self._lock == True:
            self.editableObject.data.clip_end = new_value
         else:
            SendParameterUpdate(parameter)
