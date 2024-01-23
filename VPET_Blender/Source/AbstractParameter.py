import bpy  
from mathutils import Vector, Quaternion, Color

class AbstractParameter:

    def __init__ (self, value, name, parent=None, distribute=True):
        self._type = self.to_tracer_type(type(value))
        self._value = value
        self._name = name
        self._parent = parent
        self._distribute = distribute
        self._initial_value = value
        self.hasChanged = []

        if(parent):
            self._id = len(self._parent._parameterList)

    
    def to_tracer_type(self, t):
        if t == bool:
            return 2  # BOOL
        elif t == int:
            return 3  # INT
        elif t == float:
            return 4  # FLOAT
        elif t in [Vector, Vector]:
            if len(t()) == 2:
                return 5  # Vector2
            elif len(t()) == 3:
                return 6  # Vector3
            elif len(t()) == 4:
                return 7  # Vector4
        elif t == Quaternion:
            return 8  # Quaternion
        elif t == Color:
            return 9  # Color
        elif t == str:
            return 10  # STRING
        else:
            return 100  # UNKNOWN
    
class Parameter(AbstractParameter):
    def __init__(self, value, name, parent=None, distribute=True):
        super().__init__(value, name, parent, distribute)

    def set_value(self, new_value):
        if new_value != self._value:
            self._value = new_value
            self.emitHasChanged()
    
    def emitHasChanged(self):
        for handler in self.hasChanged:
            handler(self, self._value)

   
