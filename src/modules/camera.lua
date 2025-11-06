local CameraController = {}
CameraController.__index = CameraController

function CameraController.new(event_bus)
    return setmetatable({
        event_bus = event_bus,
        parameters = {
            fov = 75,
            smoothing = 0.3,
            near_clip = 0.1,
            far_clip = 1000,
            offset = { x = 0, y = 1.6, z = -3 }
        }
    }, CameraController)
end

function CameraController:update_parameters(params)
    for key, value in pairs(params) do
        if key == "offset" then
            for axis, axisValue in pairs(value) do
                self.parameters.offset[axis] = axisValue
            end
        else
            self.parameters[key] = value
        end
    end
    self.event_bus:emit("camera_parameters_updated", self.parameters)
end

return CameraController
