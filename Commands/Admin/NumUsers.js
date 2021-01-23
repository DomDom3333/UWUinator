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
            var numUsers = 0;
            IMPORTEDBOT.bot.guilds.cache.forEach(element => {
                numUsers = numUsers + element.memberCount;
            });
            COLLECTOR.Add(`Serving ${numUsers} Users!`);        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}