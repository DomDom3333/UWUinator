const COLLECTOR = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');

const allowedAdmins = CONFIG.Admins;

module.exports = {
    name: 'reviewlog',
    description: "Plays back x amount of the current log.",
    enabled: true, //if false, command will not work
    execute(message,args){
        if(this.enabled && allowedAdmins.includes(message.author.id)){
            var promise = LOG.review(args[1])
            COLLECTOR.Add(promise);
            return promise;            
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}