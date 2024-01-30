import functools
import math
from ..AbstractParameter import Parameter
from .SceneObjectLight import SceneObjectLight

class SceneObjectSpotLight(SceneObjectLight):
    def __init__(self, obj):
        super().__init__(obj)
        range = Parameter(1, "Range", self)
        spotAngle = Parameter(math.degrees(obj.data.spot_size), "Spot", self)
        spotAngle.hasChanged.append(functools.partial(self.UpdatespotAngle, spotAngle))

    def UpdatespotAngle(self, parameter, new_value):
        self.editableObject.data.spot_size = new_value

