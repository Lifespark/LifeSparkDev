--always remember to require this package before doing anything

setmetatable(_G, {__index=age});
mobdbg = require('mobdebug');
mobdbg.start();

entity = AgeLuaComponent_GetCurrentEntity();