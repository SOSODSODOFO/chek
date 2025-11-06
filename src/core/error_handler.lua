local ErrorHandler = {}
ErrorHandler.__index = ErrorHandler

function ErrorHandler.new(logger)
    return setmetatable({ logger = logger }, ErrorHandler)
end

function ErrorHandler:wrap(name, fn)
    return function(...)
        local ok, result = xpcall(fn, debug.traceback, ...)
        if not ok then
            if self.logger then
                self.logger:error("Runtime failure", { scope = name, traceback = result })
            end
            return nil, result
        end
        return result
    end
end

return ErrorHandler
