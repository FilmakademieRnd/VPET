import functools
from ..AbstractParameter import Parameter
from .SceneObject import SceneObject
from ..serverAdapter import SendParameterUpdate;

class SceneObjectLight(SceneObject):
    def __init__(self, obj):
        super().__init__(obj)
        color = Parameter(obj.data.color, "Color", self)
        self._parameterList.append(color)
        intensity = Parameter(obj.data.energy, "Intensity", self)
        self._parameterList.append(intensity)
        color.hasChanged.append(functools.partial(self.UpdateColor, color))
        intensity.hasChanged.append(functools.partial(self.UpdateIntensity, intensity))

    def UpdateColor(self, parameter, new_value):
        if self._lock == True:
            self.editableObject.data.color = new_value
        else:
            SendParameterUpdate(parameter)

    def UpdateIntensity(self, parameter, new_value):
        if self._lock == True:
            self.editableObject.data.energy = new_value
        else:
            SendParameterUpdate(parameter)
