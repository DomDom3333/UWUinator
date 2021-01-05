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

            COLLECTOR.Add('Here are some of the servers im in: ');
            var charcount = 0;
            var randomServer = 0;
            var chosenServer = '';
            for(var i = 0;i < IMPORTEDBOT.bot.guilds.cache.size;i++){
                //randomServer = Math.floor(Math.random() * IMPORTEDBOT.BOT.guilds.cache.size);
                chosenServer = IMPORTEDBOT.bot.guilds.cache.random().name;
                if(chosenServer.length + charcount > 1900){
                    return;
                }
                charcount = charcount + chosenServer.length;
                COLLECTOR.Add(`- ${chosenServer}`);
            }
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}