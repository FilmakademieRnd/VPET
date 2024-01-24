import functools
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject


class SceneObjectCamera(SceneObject):
    def __init__(self, obj):
        super().__init__(obj)
        fov = Parameter(obj.data.angle, "Fov", self)
        aspect = Parameter(obj.data.sensor_width/obj.data.sensor_height, "Aspect", self)
        near = Parameter(obj.data.clip_start, "Near", self)
        far = Parameter(obj.data.clip_end, "Far", self)
        self._parameterList.append(fov)
        self._parameterList.append(aspect)
        self._parameterList.append(near)
        self._parameterList.append(far)
        fov.hasChanged.append(functools.partial(self.UpdateFov, fov))
        near.hasChanged.append(functools.partial(self.UpdateNear, near))
        far.hasChanged.append(functools.partial(self.UpdateFar, far))




    def UpdateFov(self, parameter, new_value):
        self.editableObject.data.angle = new_value

    def UpdateNear(self, parameter, new_value):
        self.editableObject.data.clip_start = new_value

    def UpdateFar(self, parameter, new_value):
        self.editableObject.data.clip_end = new_value
