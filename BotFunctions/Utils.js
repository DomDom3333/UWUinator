const CONFIG = require('../config.json');
const LOG = require('./Log.js');
const IMPORTEDBOT = require('../index.js');
const PREFIX = CONFIG.Prefix


var lastMessageChannel = '';
var lastMessageTime = '';
var unBanTimer = Date.now();
var warnedList = [];
var bannedChannels = [];

class warned {
    count = 1;
    warnTime = [];
    constructor(name, time) {
        this.channelName = name;
        this.warnTime.push(time);
    }
    get name(){
        return this.channelName;
    }
    get count(){
        return this.count;
    }
    get shouldBeBanned(){
        if (this.count > 4) {
            return true;
        }
        return false;
    }

    updateWarnings(){
        if (this.count < 1) {
            return;
        }
        for (let index = 0; index < this.warnTime.length; index++) {
            var times = this.warnTime[index];
            if (Date.now() - times > 15000) {
                this.subtractCount();
                this.warnTime.splice(index,1);
            }
        }
    }
    addCount(){
        this.count++;
    }
    subtractCount(){
        this.count--;
    }
}

const ALPHABET = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z']
module.exports = {
    shouldRespond(message){
        if (message.author.id == message.guild.me.id) return false;
        if (!isAllowedToSend(message)) return false; //doesnt respond if not allowed to send
        if (message.channel.nsfw) return false;
        if (message.content.includes(`<@!${message.guild.me.id}>`)) return true;
        if ((message.content.split(' ').length -1) < 2) return false;
        if (!ALPHABET.includes(message.content[0].toUpperCase())) return false;
        if (!message.content[0] === PREFIX) return false;
        if (message.author.bot) return false;
        if (message.content.length <= 1) return false;
        if (message.content.includes('https://')) return false;
        if (message.content.includes('www.')) return false;
        return true;
    },

    rateLimiter(message){
        warnedList.forEach(element => {
            element.updateWarnings()
        });

        if(Date.now()-unBanTimer > 1200000){
            bannedChannels.shift();
        }

        if(bannedChannels.includes(message.channel.id)){
            return;
        }
        var timeEllapsed = Date.now() - lastMessageTime;
    
        if(lastMessageChannel == message.channel.id && timeEllapsed < 3000 && message.author.id != message.guild.me.id){
            var channelHasWarnings = false;
            warnedList.forEach(element => {
                if (element.channelName == message.channel.id) {
                    element.addCount();
                    LOG.info(`Channel ${message.channel.name} has been warned ${element.count} times.`)
                    channelHasWarnings = true;
                }
            });
            if (!channelHasWarnings) {            
                var warnedChannel = new warned(message.channel.id,Date.now());
                warnedList.push(warnedChannel);
                LOG.info(`Channel ${message.channel.name} has been warned for the first time.`)
            }
            warnedList.forEach(element => {
                if(element.shouldBeBanned == true){
                    bannedChannels.push(message.channel.id);
                    LOG.info(`Added Channel ${message.channel.name} to banned channels`);
                }
            });
        }
        lastMessageChannel = message.channel.id;
        lastMessageTime = Date.now();
    },

    contactAdmins(message){
        CONFIG.Admins.forEach(admin => {
            try{
                IMPORTEDBOT.bot.users.cache.get(admin).send(message);
            }
            catch(err){
                LOG.error(`Failed to contact ${admin}`,err);
            }       
        });
    }
}

function isAllowedToSend(message){
    var memberPerms = message.channel.permissionsFor(message.guild.me);
    return memberPerms.has('SEND_MESSAGES')
}