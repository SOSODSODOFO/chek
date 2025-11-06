local Panel = {}
Panel.__index = Panel

function Panel.new(config, event_bus)
    return setmetatable({
        config = config,
        event_bus = event_bus,
        tabs = {},
        active_tab = nil,
        icons = {},
        filters = {
            search = "",
            tags = {}
        }
    }, Panel)
end

function Panel:add_tab(name, icon, content_provider)
    self.tabs[name] = {
        icon = icon,
        content = content_provider,
        animations = {
            duration = 0.35,
            easing = "inOutQuad"
        }
    }
    if not self.active_tab then
        self.active_tab = name
    end
end

function Panel:set_active(name)
    if self.tabs[name] then
        self.active_tab = name
        self.event_bus:emit("panel_tab_changed", { tab = name })
    end
end

function Panel:set_filter(text, tags)
    self.filters.search = text or self.filters.search
    self.filters.tags = tags or self.filters.tags
    self.event_bus:emit("panel_filter_changed", self.filters)
end

function Panel:get_active_content()
    local tab = self.tabs[self.active_tab]
    if not tab then
        return {}
    end

    local content = tab.content(self.filters)
    return content
end

return Panel
