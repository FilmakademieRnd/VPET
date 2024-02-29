import struct
import bpy  
from mathutils import Vector, Quaternion, Color
import mathutils

class AbstractParameter:

    def __init__ (self, value, name, parent=None, distribute=True):
        self._type = self.to_tracer_type(type(value))
        self._value = value
        self._name = name
        self._parent = parent
        self._distribute = distribute
        self._initial_value = value
        self._dataSize = self.getDataSize()
        self.hasChanged = []

        if(parent):
            self._id = len(parent._parameterList)
            print(str(self._id))

    
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
        
         
    def getDataSize(self):
        if self._type == 2:
            return 1
        if self._type == 3 or self._type == 4:
            return 4
        if self._type == 5:
            return 8
        if self._type == 6:
            return 12
        if self._type == 7 or self._type == 8 or self._type == 9:
            return 16
    
class Parameter(AbstractParameter):
    def __init__(self, value, name, parent=None, distribute=True):
        super().__init__(value, name, parent, distribute)

    def set_value(self, new_value):
        #if new_value != self._value:
        self._value = new_value
        self.emitHasChanged()
    
    def emitHasChanged(self):
        for handler in self.hasChanged:
            handler(self._value)

    def decodeMsg(self, msg, offset):
        type = self._type
        if type == 2 :
            floatVal = unpack('?', msg, offset)
            self.set_value(floatVal)

        if type == 4 :
            floatVal = unpack('f', msg, offset)
            self.set_value(floatVal)

        if type == 6 :
             newVector3 = mathutils.Vector(( unpack('f', msg, offset),\
                                            unpack('f', msg, offset + 4),\
                                            unpack('f', msg, offset + 8)))
             
             unityToBlenderVector3 = mathutils.Vector((newVector3[0], newVector3[2], newVector3[1]))

             self.set_value(unityToBlenderVector3)

        elif type == 8 :
            quaternion = mathutils.Quaternion(( unpack('f', msg, offset ),\
                                            unpack('f', msg, offset + 4),\
                                            unpack('f', msg, offset + 8),\
                                            unpack('f', msg, offset + 12)))
            quaternion.invert()

            unityToBlenderQuaternion = mathutils.Quaternion((quaternion[3],\
                                                                quaternion[0],\
                                                                -quaternion[2],\
                                                                -quaternion[1]))
            self.set_value(unityToBlenderQuaternion)

        elif type == 9:
             newColor = (unpack('f', msg, offset), unpack('f', msg, offset + 4), unpack('f', msg, offset + 8))

             self.set_value(newColor)


    def SerializeParameter(self):
        type = self._type
        val = self._value
        if type == 3:
            return struct.pack('i', val)
        elif type == 4:
            return struct.pack('f', val)
        elif type == 6:
            unity_vec3 = mathutils.Vector((val[0], val[2], val[1]))
            return struct.pack('3f', *unity_vec3)
        elif type == 8:
            self._parent.editableObject.rotation_mode = 'QUATERNION'
            unity_quat = mathutils.Quaternion((val[1],\
                                                val[3],\
                                                val[2],\
                                                -val[0]))
            self._parent.editableObject.rotation_mode = 'XYZ'
            #print(str(unity_quat))
            return struct.pack('4f', *unity_quat)

def unpack(type, array, offset):
    return struct.unpack_from(type, array, offset)[0]

   
