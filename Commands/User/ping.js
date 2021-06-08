const Collector = require('../../BotFunctions/MessageCollector.js');
const Utils = require('../../BotFunctions/Utils.js');
const IMPORTEDBOT = require('../../index.js');

module.exports = {
    name: 'ping',
    description: "says pong!",
    enabled: true, //if false, command will not work
    async execute(message,args){
        if(this.enabled){
            Collector.Add('pong');
            Collector.Add('My ping to Discord is: ' + IMPORTEDBOT.bot.ws.ping + 'ms');
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}