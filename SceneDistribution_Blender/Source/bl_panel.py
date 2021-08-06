import bpy

## Interface
#  
class VPET_PT_Panel(bpy.types.Panel):
    bl_idname = "VPET_PT_PANEL"
    bl_label = "VPET"
    bl_category = "VPET Addon"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    
    def draw(self, context):
        layout = self.layout
        #scene = context.scene
        
        row = layout.row()
        row.operator('object.zmq_install', text = 'Pip Install ZMQ')
        row.operator('object.setup_vpet', text='Setup Scene for VPET')
        
        row = layout.row()
        row.operator('object.zmq_distribute', text = "Do Distribute")
        row.operator('object.zmq_stopdistribute', text = "Stop Distribute")

        row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'vpet_collection')
        row = layout.row()
        row.prop(bpy.context.scene.vpet_properties, 'edit_collection')