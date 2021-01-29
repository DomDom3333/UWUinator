const Discord = require('discord.js');
const CONFIG = require('./config.json');
const UTILS = require('./BotFunctions/Utils.js');
const LOG = require('./BotFunctions/Log.js');
const COLLECTOR = require('./BotFunctions/MessageCollector.js');
const MESSAGECENTER = require('./BotFunctions/MessageCenter.js');
const Utils = require('./BotFunctions/Utils.js');
const bot = new Discord.Client();
const botRole = '776942635595857955';


bot.login(CONFIG.Token);
PREFIX = CONFIG.Prefix;


bot.once('ready', () => {
    this.bot = bot
    bot.user.setActivity(CONFIG.ReadyMessage)
    LOG.info("\nWe UwU-ing Now!!")//successful startup log
});

bot.on('message', async message => {   
    try{
        if(UTILS.shouldRespond(message)){           
            if (!UTILS.rateLimiter(message)){
                return;
            }
            
            await MESSAGECENTER.messageHandler(message,PREFIX);
            
            var msg = COLLECTOR.Return();
            
            await Promise.all(COLLECTOR.isWaiting());
            if (COLLECTOR.hasAttach()) {
                message.channel.startTyping();
                var attaches = COLLECTOR.ReturnAttachData();
                if (fs.existsSync(attaches[0])) {
                    message.channel.send(msg,{
                        files: [{
                            attachment: attaches[0],
                            name: attaches[1]
                            }]
                    });
                    COLLECTOR.Clear();                      
                }
                else{
                    LOG.error(`Failed to find resulting image at ${attaches[0]}.`)
                }
            }
            else if (msg != ''){
                message.channel.send(msg);
                COLLECTOR.Clear();
            }
            message.channel.stopTyping();
        }
    }
    catch(e){
        console.log('~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SOMETHING WENT WRONG!!!!!!!!!!!!!!!!!!!')
        LOG.error(message.content, e);
        UTILS.contactAdmins(`Something went wrong with me!\n${message.content}\n${e}`);
        console.log('~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SOMETHING WENT WRONG!!!!!!!!!!!!!!!!!!!')
    }
});

bot.on('guildCreate', guild => {
    LOG.info(`NEW SERVER JOINED!!!!!!!!\n- Name: ${guild.name}\n- Member Count: ${guild.memberCount}\nI'm now in ${bot.guilds.cache.size} Servers!`);
    Utils.contactAdmins(`NEW SERVER JOINED!!!!!!!!\n- Name: ${guild.name}\n- Member Count: ${guild.memberCount}\nI'm now in ${bot.guilds.cache.size} Servers!`);
});

bot.on("guildDelete", guild => {
    LOG.info(`Got booted from a Server!!!!!!!!\n- Name: ${guild.name}\n- Member Count: ${guild.memberCount}\nI'm now in ${bot.guilds.cache.size} Servers!`);
    Utils.contactAdmins(`Got booted from a Server!!!!!!!!\n- Name: ${guild.name}\n- Member Count: ${guild.memberCount}\nI'm now in ${bot.guilds.cache.size} Servers!`);
})

bot.on('error', async error =>{
    LOG.error('A fatal error occoured!. Bot will no longer be opertational!',error);

    var LogPromise = await LOG.review(50);
    if (LogPromise.length > 1500) {
        LogPromise = LogPromise.substring(0,1500);
    }
   UTILS.contactAdmins(`A fatal error occoured!. Bot will no longer be opertational!\nSending last Log entries:\n${LogPromise}\n${error}`);
});