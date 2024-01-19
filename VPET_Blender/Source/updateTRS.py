import bpy
import time

class RealTimeUpdaterOperator(bpy.types.Operator):
    bl_idname = "wm.real_time_updater"
    bl_label = "Real-Time Updater"

    _timer = None

    def modal(self, context, event):
        if event.type == 'TIMER':
            self.check_for_updates(context)
        return {'PASS_THROUGH'}

    def execute(self, context):
        wm = context.window_manager
        self.start_transforms = {obj.name: (obj.location.copy(), obj.rotation_euler.copy(), obj.scale.copy()) for obj in bpy.context.scene.objects}
        self._timer = wm.event_timer_add(0.1, window=context.window)
        wm.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def check_for_updates(self, context):
        for obj in bpy.context.scene.objects:
            # Check if the object has a starting transform stored
            if obj.name in self.start_transforms:
                start_location, start_rotation, start_scale = self.start_transforms[obj.name]

                # Compare the current transform with the starting one
                if (obj.location - start_location).length > 0.0001:
                    print(f"Object '{obj.name}' has been modified:")
                    print("  Position has changed: " + str(obj.location))

                rotation_difference = (start_rotation.to_matrix().inverted() @ obj.rotation_euler.to_matrix()).to_euler()
                if any(abs(value) > 0.0001 for value in rotation_difference):
                    print(f"Object '{obj.name}' has been modified:")
                    print("  Rotation has changed.")

                if (obj.scale - start_scale).length > 0.0001:
                    print(f"Object '{obj.name}' has been modified:")
                    print("  Scale has changed.")

                # Update the starting transform
                self.start_transforms[obj.name] = (obj.location.copy(), obj.rotation_euler.copy(), obj.scale.copy())

    def cancel(self, context):
        wm = context.window_manager
        wm.event_timer_remove(self._timer)

def register():
    bpy.utils.register_class(RealTimeUpdaterOperator)

def unregister():
    bpy.utils.unregister_class(RealTimeUpdaterOperator)

if __name__ == "__main__":
    register()
    bpy.ops.wm.real_time_updater('INVOKE_DEFAULT')
