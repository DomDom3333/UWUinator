const COLLECTOR = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');
const IMPORTEDBOT = require('../../index.js');

const allowedAdmins = CONFIG.Admins;

module.exports = {
    name: 'numusers',
    description: 'Shuts down the Bot.',
    enabled: true, //if false, command will not work
    execute(message,args){
        if(this.enabled && allowedAdmins.includes(message.author.id)){
            COLLECTOR.Add(`Serving ${IMPORTEDBOT.bot.users.cache.size} Users!`);
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}