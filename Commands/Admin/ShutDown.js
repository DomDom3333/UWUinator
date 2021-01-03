const COLLECTOR = require('../../BotFunctions/MessageCollector.js');
const LOG = require('../../BotFunctions/Log.js');
const CONFIG = require('../../config.json');

const allowedAdmins = CONFIG.Admins;

var shutdownRequests = 0;
var lastSDRequestTime = Date.now();

module.exports = {
    name: 'shutdown',
    description: 'Shuts down the Bot.',
    enabled: true, //if false, command will not work
    execute(message,args){
        if(this.enabled && allowedAdmins.includes(message.author.id)){
            if(Date.now() - lastSDRequestTime > 10000){
                shutdownRequests = 0;
            }
            if(shutdownRequests == 0){
                COLLECTOR.Add(`Are you sure you want to me down? To confirm, resend the command within 10 senconds.`);
                LOG.info(`First shutdown request recieved by ${message.author.username}`);
                shutdownRequests++;
            }
            else if(shutdownRequests == 1){
                LOG.info(`User ${message.author.username} sent a shutdown command. Closing down.\nGoodbye!`);
                process.exit(1);
            }
            lastSDRequestTime = Date.now();
        }
        else{
            Collector.Add("This command is currently DISABLED");
        }
    }
}