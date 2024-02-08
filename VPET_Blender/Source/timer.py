import bpy
import time  # Ensure time module is imported

class TimerClass:
    s_timestepsBase = 128

    def __init__(self, framerate=60):
        self.framerate = framerate
        self.m_timesteps = (TimerClass.s_timestepsBase // self.framerate) * self.framerate
        self.last_update_time = time.time()
        self.start_time = time.time()  # Initialize start_time for continuous measurement
        self.time_60_start_time = None  # To measure time from 60 to 119
        self.accumulated_increments = 0.0  # To accumulate fractional increments

    def update_time(self):
        global vpet
        current_time = time.time()
        elapsed = current_time - self.last_update_time  # Elapsed time since last update
        increments = elapsed * self.framerate  # Calculate potential fractional increments
        
        # Accumulate increments (including fractional parts)
        self.accumulated_increments += increments
        
        # Determine how many whole increments to apply
        whole_increments = int(self.accumulated_increments)
        
        # Adjust accumulated increments to keep only the fractional part
        self.accumulated_increments -= whole_increments
        
        # Update vpet.time with whole increments
        new_time = vpet.time + whole_increments
        
        """
        # Check for milestones within the update span
        if vpet.time < 60 <= new_time:
            # Just crossed 60
            self.time_60_start_time = current_time  # Mark the time when crossing 60
            elapsed_time_to_60 = self.time_60_start_time - self.start_time
            print(f"It took {elapsed_time_to_60} seconds for vpet.time to reach 60.")
        if vpet.time < 119 <= new_time:
            # Just crossed 119
            if self.time_60_start_time:
                elapsed_time_from_60_to_119 = current_time - self.time_60_start_time
                print(f"It took {elapsed_time_from_60_to_119} seconds for vpet.time to go from 60 to 119.")
                # Consider resetting for the next cycle if needed
        """

        # Handle cycle reset and ensure vpet.time remains an integer
        if new_time >= self.m_timesteps - 1:
            vpet.time = int(new_time % self.m_timesteps)  # Loop back if exceeding m_timesteps
            self.start_time = current_time  # Reset start time for the new cycle
            self.time_60_start_time = None  # Reset to measure next span from 60 to 119 again
        else:
            vpet.time = int(new_time)
        
        self.last_update_time = current_time  # Update the last_update_time for the next cycle

class TimerModalOperator(bpy.types.Operator):
    """Operator to run a timer at specified framerate"""
    bl_idname = "wm.timer_modal_operator"
    bl_label = "Timer Modal Operator"

    _timer = None
    my_instance = TimerClass(framerate=60)  # Create an instance of TimerClass

    def modal(self, context, event):
        if event.type == 'TIMER':
            self.my_instance.update_time()
        return {'PASS_THROUGH'}

    def execute(self, context):
        wm = context.window_manager
        global vpet
        vpet = bpy.context.window_manager.vpet_data
        self._timer = wm.event_timer_add(1.0 / self.my_instance.framerate, window=context.window)
        wm.modal_handler_add(self)
        return {'RUNNING_MODAL'}

    def cancel(self, context):
        wm = context.window_manager
        if self._timer:
            wm.event_timer_remove(self._timer)
