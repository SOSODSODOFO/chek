-- GameSense Auto Buy script with budget manager and prioritised purchase queue

local globals_realtime = globals.realtime
local globals_curtime = globals.curtime
local globals_maxplayers = globals.maxplayers
local globals_tickcount = globals.tickcount
local globals_tickinterval = globals.tickinterval
local globals_mapname = globals.mapname

local client_set_event_callback = client.set_event_callback
local client_console_cmd = client.exec
local client_userid_to_entindex = client.userid_to_entindex
local client_delay_call = client.delay_call

local ui_new_checkbox = ui.new_checkbox
local ui_new_slider = ui.new_slider
local ui_new_combobox = ui.new_combobox
local ui_new_multiselect = ui.new_multiselect
local ui_new_label = ui.new_label
local ui_set = ui.set
local ui_get = ui.get
local ui_set_visible = ui.set_visible
local ui_set_callback = ui.set_callback

local entity_get_local_player = entity.get_local_player
local entity_get_prop = entity.get_prop

local table_insert = table.insert
local table_sort = table.sort
local table_concat = table.concat
local math_floor = math.floor

local tab = "Auto Buy"
local preview_budget

local function format_currency(amount)
    local remainder = amount
    local parts = {}
    repeat
        local chunk = remainder % 1000
        remainder = math_floor(remainder / 1000)
        if remainder > 0 then
            table_insert(parts, 1, string.format("%03d", chunk))
        else
            table_insert(parts, 1, tostring(chunk))
        end
    until remainder == 0
    return "$" .. table_concat(parts, ",")
end

local function make_item(name, command, price)
    return {
        name = name,
        command = command,
        price = price
    }
end

local primary_weapons = {
    make_item("-", "", 0),
    make_item("AK-47 / M4A4", "buy ak47; buy m4a1; ", 2700),
    make_item("M4A1-S", "buy m4a1_silencer; ", 2900),
    make_item("FAMAS / Galil", "buy famas; buy galilar; ", 2050),
    make_item("AUG / SG553", "buy aug; buy sg556; ", 3150),
    make_item("SSG 08", "buy ssg08; ", 1700),
    make_item("AWP", "buy awp; ", 4750),
    make_item("Auto-Sniper", "buy scar20; buy g3sg1; ", 5000),
    make_item("Nova", "buy nova; ", 1050),
    make_item("XM1014", "buy xm1014; ", 2000),
    make_item("MAG-7 / Sawed-Off", "buy mag7; buy sawedoff; ", 1300),
    make_item("PP-Bizon", "buy bizon; ", 1400),
    make_item("UMP-45", "buy ump45; ", 1200),
    make_item("MP7 / MP5", "buy mp7; buy mp5sd; ", 1500),
    make_item("P90", "buy p90; ", 2350)
}

local secondary_weapons = {
    make_item("-", "", 0),
    make_item("Glock / USP-S / P2000", "buy glock; buy usp_silencer; buy hkp2000; ", 200),
    make_item("Dual Berettas", "buy elite; ", 400),
    make_item("P250", "buy p250; ", 300),
    make_item("Five-SeveN / Tec-9 / CZ75", "buy fn57; buy tec9; buy cz75a; ", 500),
    make_item("Desert Eagle / R8", "buy deagle; buy revolver; ", 700)
}

local gear_items = {
    armor = make_item("Kevlar", "buy vest; ", 650),
    helmet = make_item("Helmet", "buy vesthelm; ", 1000),
    defuser = make_item("Defuse Kit", "buy defuser; ", 400),
    taser = make_item("Zeus x27", "buy taser; ", 200),
    knife = make_item("Knife", "buy knife; ", 1500)
}

local grenade_items = {
    he = make_item("HE Grenade", "buy hegrenade; ", 300),
    flash = make_item("Flashbang", "buy flashbang; ", 200),
    smoke = make_item("Smoke Grenade", "buy smokegrenade; ", 300),
    molotov = make_item("Molotov / Incendiary", "buy molotov; buy incgrenade; ", 600)
}

local enable_checkbox = ui_new_checkbox("MISC", tab, "Enable auto buy")
local info_label = ui_new_label("MISC", tab, "Configure automatic purchases below")

local primary_enable = ui_new_checkbox("MISC", tab, "Buy primary weapon")
local primary_choice = ui_new_combobox("MISC", tab, "Primary weapon", (function()
    local names = {}
    for i = 1, #primary_weapons do
        names[i] = primary_weapons[i].name
    end
    return names
end)())

local secondary_enable = ui_new_checkbox("MISC", tab, "Buy secondary weapon")
local secondary_choice = ui_new_combobox("MISC", tab, "Secondary weapon", (function()
    local names = {}
    for i = 1, #secondary_weapons do
        names[i] = secondary_weapons[i].name
    end
    return names
end)())

local armor_enable = ui_new_checkbox("MISC", tab, "Buy Kevlar")
local helmet_enable = ui_new_checkbox("MISC", tab, "Buy Helmet")
local defuse_enable = ui_new_checkbox("MISC", tab, "Buy Defuse Kit")
local taser_enable = ui_new_checkbox("MISC", tab, "Buy Zeus")
local knife_enable = ui_new_checkbox("MISC", tab, "Buy Knife (if available)")

local grenade_multi = ui_new_multiselect("MISC", tab, "Grenades", {
    grenade_items.he.name,
    grenade_items.flash.name,
    grenade_items.smoke.name,
    grenade_items.molotov.name
})
local flash_count = ui_new_slider("MISC", tab, "Flashbang count", 0, 2, 2)

local priority_primary = ui_new_slider("MISC", tab, "Priority: Primary", 1, 6, 2)
local priority_secondary = ui_new_slider("MISC", tab, "Priority: Secondary", 1, 6, 1)
local priority_grenades = ui_new_slider("MISC", tab, "Priority: Grenades", 1, 6, 3)
local priority_armor = ui_new_slider("MISC", tab, "Priority: Armor", 1, 6, 4)
local priority_utility = ui_new_slider("MISC", tab, "Priority: Utility", 1, 6, 5)
local budget_label = ui_new_label("MISC", tab, "Budget manager: ready")

local function has_flash_selected()
    local selected = ui_get(grenade_multi)
    if type(selected) ~= "table" then
        return false
    end

    for i = 1, #selected do
        if selected[i] == grenade_items.flash.name then
            return true
        end
    end

    return false
end

local function update_visibility()
    local enabled = ui_get(enable_checkbox)
    ui_set_visible(info_label, enabled)
    ui_set_visible(primary_enable, enabled)
    ui_set_visible(primary_choice, enabled and ui_get(primary_enable))
    ui_set_visible(secondary_enable, enabled)
    ui_set_visible(secondary_choice, enabled and ui_get(secondary_enable))
    ui_set_visible(armor_enable, enabled)
    ui_set_visible(helmet_enable, enabled)
    ui_set_visible(defuse_enable, enabled)
    ui_set_visible(taser_enable, enabled)
    ui_set_visible(knife_enable, enabled)
    ui_set_visible(grenade_multi, enabled)
    ui_set_visible(flash_count, enabled and has_flash_selected())
    ui_set_visible(priority_primary, enabled)
    ui_set_visible(priority_secondary, enabled)
    ui_set_visible(priority_grenades, enabled)
    ui_set_visible(priority_armor, enabled)
    ui_set_visible(priority_utility, enabled)
    ui_set_visible(budget_label, enabled)

    if not enabled then
        ui_set(budget_label, "Budget manager: disabled")
        return
    end

    preview_budget()
end

local function get_selection(list, name)
    for i = 1, #list do
        if list[i].name == name then
            return list[i]
        end
    end
end

local function collect_purchase_entries()
    local queue = {}

    if ui_get(primary_enable) then
        local selection = get_selection(primary_weapons, ui_get(primary_choice))
        if selection and selection.command ~= "" then
            table_insert(queue, {
                priority = ui_get(priority_primary),
                price = selection.price,
                command = selection.command,
                name = selection.name
            })
        end
    end

    if ui_get(secondary_enable) then
        local selection = get_selection(secondary_weapons, ui_get(secondary_choice))
        if selection and selection.command ~= "" then
            table_insert(queue, {
                priority = ui_get(priority_secondary),
                price = selection.price,
                command = selection.command,
                name = selection.name
            })
        end
    end

    local selected_grenades = ui_get(grenade_multi) or {}
    if #selected_grenades > 0 then
        for i = 1, #selected_grenades do
            local grenade_name = selected_grenades[i]
            for key, item in pairs(grenade_items) do
                if item.name == grenade_name then
                    local count = 1
                    if key == "flash" then
                        count = ui_get(flash_count)
                    end
                    for _ = 1, count do
                        table_insert(queue, {
                            priority = ui_get(priority_grenades),
                            price = item.price,
                            command = item.command,
                            name = item.name
                        })
                    end
                end
            end
        end
    end

    if ui_get(armor_enable) then
        table_insert(queue, {
            priority = ui_get(priority_armor),
            price = gear_items.armor.price,
            command = gear_items.armor.command,
            name = gear_items.armor.name
        })
    end

    if ui_get(helmet_enable) then
        table_insert(queue, {
            priority = ui_get(priority_armor),
            price = gear_items.helmet.price,
            command = gear_items.helmet.command,
            name = gear_items.helmet.name
        })
    end

    if ui_get(defuse_enable) then
        table_insert(queue, {
            priority = ui_get(priority_utility),
            price = gear_items.defuser.price,
            command = gear_items.defuser.command,
            name = gear_items.defuser.name
        })
    end

    if ui_get(taser_enable) then
        table_insert(queue, {
            priority = ui_get(priority_utility),
            price = gear_items.taser.price,
            command = gear_items.taser.command,
            name = gear_items.taser.name
        })
    end

    if ui_get(knife_enable) then
        table_insert(queue, {
            priority = ui_get(priority_utility),
            price = gear_items.knife.price,
            command = gear_items.knife.command,
            name = gear_items.knife.name
        })
    end

    return queue
end

local function build_purchase_queue(money)
    local queue = collect_purchase_entries()

    table_sort(queue, function(a, b)
        if a.priority == b.priority then
            return a.price > b.price
        end
        return a.priority < b.priority
    end)

    local total_cost = 0
    for i = 1, #queue do
        total_cost = total_cost + queue[i].price
    end

    local command_buffer = {}
    local spent = 0

    for i = 1, #queue do
        if spent + queue[i].price <= money then
            spent = spent + queue[i].price
            table_insert(command_buffer, queue[i].command)
        end
    end

    if total_cost == 0 then
        ui_set(budget_label, "Budget manager: nothing selected")
    else
        ui_set(budget_label, string.format("Budget manager: %s / %s", format_currency(spent), format_currency(total_cost)))
    end

    return table_concat(command_buffer, ""), spent
end

preview_budget = function()
    if not ui_get(enable_checkbox) then
        ui_set(budget_label, "Budget manager: disabled")
        return
    end

    local lp = entity_get_local_player()
    local money = lp and (entity_get_prop(lp, "m_iAccount") or 0) or 0
    build_purchase_queue(money)
end

local function on_settings_change()
    if ui_get(enable_checkbox) then
        preview_budget()
    end
end

ui_set_callback(primary_choice, on_settings_change)
ui_set_callback(secondary_choice, on_settings_change)
ui_set_callback(armor_enable, on_settings_change)
ui_set_callback(helmet_enable, on_settings_change)
ui_set_callback(defuse_enable, on_settings_change)
ui_set_callback(taser_enable, on_settings_change)
ui_set_callback(knife_enable, on_settings_change)
ui_set_callback(flash_count, on_settings_change)
ui_set_callback(priority_primary, on_settings_change)
ui_set_callback(priority_secondary, on_settings_change)
ui_set_callback(priority_grenades, on_settings_change)
ui_set_callback(priority_armor, on_settings_change)
ui_set_callback(priority_utility, on_settings_change)

ui_set_callback(enable_checkbox, update_visibility)
ui_set_callback(primary_enable, update_visibility)
ui_set_callback(secondary_enable, update_visibility)
ui_set_callback(grenade_multi, update_visibility)

update_visibility()

local function try_purchase()
    if not ui_get(enable_checkbox) then
        return
    end

    local local_player = entity_get_local_player()
    if not local_player then
        return
    end

    local money = entity_get_prop(local_player, "m_iAccount") or 0
    local command_string = build_purchase_queue(money)

    if command_string ~= "" then
        client_delay_call(0.03, client_console_cmd, command_string .. "use weapon_knife;")
    end
end

local function on_event(e)
    if e.userid == nil then
        return
    end

    local lp = entity_get_local_player()
    if lp == nil then
        return
    end

    if client_userid_to_entindex(e.userid) ~= lp then
        return
    end

    try_purchase()
end

client_set_event_callback("player_spawn", on_event)
client_set_event_callback("round_prestart", try_purchase)

