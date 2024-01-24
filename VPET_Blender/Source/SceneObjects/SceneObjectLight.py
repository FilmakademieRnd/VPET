import functools
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject

class SceneObjectLight(SceneObject):
    def __init__(self, obj):
        super().__init__(obj)
        color = Parameter(obj.data.color, "Color", self)
        intensity = Parameter(obj.data.energy, "Intensity", self)
        self._parameterList.append(color)
        self._parameterList.append(intensity)
        color.hasChanged.append(functools.partial(self.UpdateColor, color))
        intensity.hasChanged.append(functools.partial(self.UpdateIntensity, intensity))

    def UpdateColor(self, parameter, new_value):
        self.editableObject.data.color = new_value

    def UpdateIntensity(self, parameter, new_value):
        self.editableObject.data.energy = new_value
