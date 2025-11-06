local System = {}
System.__index = System

local Logger = require("src.core.logger")
local ConfigManager = require("src.core.config")
local EventBus = require("src.core.event_bus")
local UpdateLoop = require("src.core.update_loop")
local ErrorHandler = require("src.core.error_handler")

local MovementController = require("src.control.movement")
local EnvironmentController = require("src.control.environment")
local DataTracker = require("src.analytics.data_tracking")
local VisualDisplay = require("src.analytics.visual_display")
local HighlightSystem = require("src.analytics.highlighting")

local Panel = require("src.interface.panel")
local Preview = require("src.interface.preview")
local Themes = require("src.interface.themes")
local Backgrounds = require("src.interface.backgrounds")

local Automation = require("src.modules.automation")
local ObjectTools = require("src.modules.object_tools")
local EnvironmentModifiers = require("src.modules.environment_modifiers")
local PerformanceAnalyzer = require("src.modules.performance")
local CameraController = require("src.modules.camera")
local Monitoring = require("src.modules.monitoring")

local Renderer = require("src.rendering.renderer")

function System.new()
    local logger = Logger.new(Logger.levels.INFO)
    local event_bus = EventBus.new()
    local config = ConfigManager.new({
        orbit_radius = 8,
        orbit_speed = 0.6,
        time = 0
    })
    local update_loop = UpdateLoop.new(logger)
    local error_handler = ErrorHandler.new(logger)

    local movement = MovementController.new(config, event_bus, logger)
    local environment = EnvironmentController.new(config, event_bus, logger)
    local tracker = DataTracker.new(event_bus, logger)
    local display = VisualDisplay.new(config, event_bus)
    local highlight = HighlightSystem.new(event_bus, config)

    local panel = Panel.new(config, event_bus)
    local preview = Preview.new(event_bus)
    local themes = Themes.new(event_bus)
    local backgrounds = Backgrounds.new(event_bus)

    local automation = Automation.new(event_bus, logger)
    local objects = ObjectTools.new(event_bus)
    local env_modifiers = EnvironmentModifiers.new(event_bus)
    local performance = PerformanceAnalyzer.new(event_bus, logger)
    local camera = CameraController.new(event_bus)
    local monitoring = Monitoring.new(event_bus, logger)
    monitoring:bind()

    local renderer = Renderer.new(event_bus, logger)

    local components = {
        movement,
        environment,
        tracker,
        display,
        highlight,
        automation,
        objects,
        env_modifiers,
        performance,
        renderer
    }

    for _, component in ipairs(components) do
        update_loop:register(component)
    end

    panel:add_tab("movement", "icons/movement.png", function(filters)
        return {
            title = "Movement Controls",
            filters = filters,
            parameters = {
                speed = movement.speed_limit,
                height = movement.height_target,
                mode = movement.navigation_mode
            }
        }
    end)

    panel:add_tab("environment", "icons/environment.png", function(filters)
        return {
            title = "Environment Settings",
            filters = filters,
            collision = environment.collision_settings,
            modifiers = environment.object_speed_modifiers
        }
    end)

    preview:register_demo("smooth-transition", function()
        movement:tune_parameters({ smoothing = 0.4 })
        return "Smooth transition demo triggered"
    end)

    highlight:add_method("depth-aware", function()
        return {
            { id = "enemy-1", tag = "enemy", depth = 3.2 },
            { id = "ally-2", tag = "ally", depth = 5.8 }
        }
    end)

    renderer:add_batch("hud", {})

    env_modifiers:add("wind", function(state)
        state.wind = { x = 0.3, y = 0, z = -0.1 }
        return state
    end)

    automation:register("heartbeat", function(context)
        performance:record("heartbeat:init", 0)
        while true do
            local dt_update = coroutine.yield()
            performance:record("heartbeat", dt_update or 0)
        end
    end)

    automation:start("heartbeat")

    return setmetatable({
        logger = logger,
        config = config,
        event_bus = event_bus,
        update_loop = update_loop,
        error_handler = error_handler,
        movement = movement,
        environment = environment,
        tracker = tracker,
        display = display,
        highlight = highlight,
        panel = panel,
        preview = preview,
        themes = themes,
        backgrounds = backgrounds,
        automation = automation,
        objects = objects,
        env_modifiers = env_modifiers,
        performance = performance,
        camera = camera,
        monitoring = monitoring,
        renderer = renderer
    }, System)
end

function System:update(dt)
    local time = (self.config:get("time") or 0) + dt
    self.config:set("time", time)
    self.update_loop:step(dt)
end

function System:apply_environment_modifiers()
    local state = {
        gravity = { 0, -9.81, 0 },
        temperature = 22
    }
    return self.env_modifiers:apply(state)
end

return System
