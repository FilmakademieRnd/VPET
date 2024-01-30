import bpy

class OBJECT_OT_single_select(bpy.types.Operator):
    bl_idname = "object.single_select"
    bl_label = "Single Object Selection and Print"
    bl_options = {'REGISTER'}

    _timer = None
    last_selected_objects = set()  # Variable to store the last selected object

    def modal(self, context, event):
        if event.type == 'TIMER':
            current_selected_objects = set(context.selected_objects)

            # Check if multiple objects are selected
            if len(current_selected_objects) > 1:
                # Check if there was a previously selected object before multiple selection attempt
                previously_selected = self.last_selected_objects
                if previously_selected:
                    # Print the deselected object(s) only if they were previously selected
                    for obj in previously_selected:
                        print(f"Deselected object: {obj.name}")

                # Deselect all objects
                for obj in current_selected_objects:
                    obj.select_set(False)

                # Clear the last selected objects set
                self.last_selected_objects = set()

            else:
                # Check for deselection
                deselected_objects = self.last_selected_objects - current_selected_objects
                for obj in deselected_objects:
                    for scene_obj in vpet.SceneObjects:
                        if obj == scene_obj.editableObject:
                            print(f"Deselected object: {obj.name}")

                # Check for new selection
                newly_selected_objects = current_selected_objects - self.last_selected_objects
                for obj in newly_selected_objects:
                    for scene_obj in vpet.SceneObjects:
                        if obj == scene_obj.editableObject:
                            print(f"Selected object: {obj.name}")

                # Update the last selected objects set
                self.last_selected_objects = current_selected_objects

        return {'PASS_THROUGH'}

    def execute(self, context):
        global vpet
        vpet = bpy.context.window_manager.vpet_data
        self._timer = context.window_manager.event_timer_add(0.1, window=context.window)
        context.window_manager.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def cancel(self, context):
        if self._timer:
            context.window_manager.event_timer_remove(self._timer)

