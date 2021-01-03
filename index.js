const Discord = require('discord.js');
const CONFIG = require('./config.json');
const UTILS = require('./BotFunctions/Utils.js');
const LOG = require('./BotFunctions/Log.js');
const COLLECTOR = require('./BotFunctions/MessageCollector.js');
const MESSAGECENTER = require('./BotFunctions/MessageCenter.js');
const bot = new Discord.Client();
const botRole = '776942635595857955';


bot.login(CONFIG.Token);
PREFIX = CONFIG.Prefix;


bot.once('ready', () => {
    this.bot = bot
    bot.user.setActivity(CONFIG.ReadyMessage)
    LOG.info("BeeP BooP Bot Online Now")//successful startup log
});

bot.on('message', async message => {   
    try{
        if(UTILS.shouldRespond(message)){                       
            await MESSAGECENTER.messageHandler(message,PREFIX);
            
            var msg = COLLECTOR.Return();
            
            if (msg != ''){
                message.channel.startTyping();
                message.channel.send(msg);
                COLLECTOR.Clear();
            }
            message.channel.stopTyping();
        }
    }
    catch(e){
        console.log('~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SOMETHING WENT WRONG!!!!!!!!!!!!!!!!!!!')
        LOG.error(message.content, e);
        console.log('~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SOMETHING WENT WRONG!!!!!!!!!!!!!!!!!!!')
    }
});

bot.on('guildCreate', guild => {
    LOG.info(`NEW GUILD JOINED!!!!!!!!\n- Name: ${guild.name}\n- Member Count: ${guild.memberCount}`);
});

bot.on('error', async error =>{
    LOG.error('A fatal error occoured!. Bot will no longer be opertational!',error);

    var LogPromise = await LOG.review(50);
    if (LogPromise.length > 1500) {
        LogPromise = LogPromise.substring(0,1500);
    }
    CONFIG.Admins.forEach(admin => {
        try{
            IMPORTEDBOT.bot.users.cache.get(admin).send(`A fatal error occoured!. Bot will no longer be opertational!\nSending last Log entries:\n${LogPromise}\n${error}`);
        }
        catch(err){
            LOG.error(`Failed to contact ${admin}`,err);
        }       
    });
});