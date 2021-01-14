const COLLECTOR = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');
const IMPORTEDBOT = require('../../index.js');

const allowedAdmins = CONFIG.Admins;

module.exports = {
    name: 'listguilds',
    description: 'Shuts down the Bot.',
    enabled: true, //if false, command will not work
    execute(message,args){
        if(this.enabled && allowedAdmins.includes(message.author.id)){
            var ServerList = 'Here are some of the servers im in: ';
            var charcount = 0;
            var chosenServer = '';
            for(var i = 0;i < IMPORTEDBOT.bot.guilds.cache.size;i++){
                chosenServer = IMPORTEDBOT.bot.guilds.cache.random().name;
                if (!ServerList.includes(chosenServer)) {
                    if(chosenServer.length + charcount > 1900){
                        return;
                    }
                    charcount = charcount + chosenServer.length;
                    ServerList = `${ServerList}\n- ${chosenServer}`                 
                }
            }
            COLLECTOR.Add(ServerList);
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}