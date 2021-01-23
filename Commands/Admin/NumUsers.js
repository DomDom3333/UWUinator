const COLLECTOR = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');
const IMPORTEDBOT = require('../../index.js');

const allowedAdmins = CONFIG.Admins;

module.exports = {
    name: 'numusers',
    description: 'Shuts down the Bot.',
    enabled: true, //if false, command will not work
    async execute(message,args){
        if(this.enabled && allowedAdmins.includes(message.author.id)){
            var numUsers = 0;
            IMPORTEDBOT.bot.guilds.cache.forEach(element => {
                if (!isNaN(element.memberCount)) {
                    numUsers = numUsers + element.memberCount; //Can be expanded to differentiate between bots and humans once verified! https://stackoverflow.com/questions/64559390/none-of-my-discord-js-guildmember-events-are-emitting-my-user-caches-are-basica
                }
            });
            COLLECTOR.Add(`Serving ${numUsers} Humans!`);        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}