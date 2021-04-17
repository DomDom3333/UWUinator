const CONFIG = require('../config.json');
const LOG = require('./Log.js');
const IMPORTEDBOT = require('../index.js');
const PREFIX = CONFIG.Prefix;
const fs = require('fs');

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

const ALPHABET = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', PREFIX]
module.exports = {
    shouldRespond(message){
        if (message.author.bot) return false;
        if (message.guild === null) return false; //is a DM?
        if (!message.content) return false; //Has text? (could be only image)
        if (!isAllowedToSend(message)) return false; //doesnt respond if not allowed to send to channel. Otherwise discord.js errors
        if (message.channel.nsfw) return false;
        if (message.mentions.users.size > 0) { //is tagged
            var mention = message.mentions.users.first()
            if (mention.id == message.guild.me.id){
                return true;
            }
        }
        if (!ALPHABET.includes(message.content[0].toUpperCase())) return false;
        if (message.content.includes('https://')) return false;
        if (message.content.includes('www.')) return false;
        return true;
    },

    rateLimiter(message){
        var pass = true;
        
        if(Date.now()-unBanTimer > 1200000){
            bannedChannels.shift();
            unBanTimer = Date.now();
        }

        warnedList.forEach(element => {
            element.updateWarnings();
            if (element.channelName == message.channel.id && element.shouldBeBanned == true) {
                pass = false;
            }
        });

        if(bannedChannels.includes(message.channel.id || !pass)){
            return false;
        }
        var timeEllapsed = Date.now() - lastMessageTime;
    
        if(lastMessageChannel == message.channel.id && timeEllapsed < 3000 && message.author.id != message.guild.me.id){
            var channelHasWarnings = false;
            warnedList.forEach(element => {
                if (element.channelName == message.channel.id) {
                    element.addCount();
                    //LOG.info(`Channel ${message.channel.name} has been warned ${element.count} times.`)
                    channelHasWarnings = true;
                    if(element.shouldBeBanned == true){
                        bannedChannels.push(message.channel.id);
                        LOG.info(`Added Channel ${message.channel.name} to banned channels`);
                        return false;
                    }
                }
            });
            if (!channelHasWarnings) {            
                var warnedChannel = new warned(message.channel.id,Date.now());
                warnedList.push(warnedChannel);
                //LOG.info(`Channel ${message.channel.name} has been warned for the first time.`)
            }
        }
        lastMessageChannel = message.channel.id;
        lastMessageTime = Date.now();
        return true;
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
    },

    saveJsonFile(inputData, Path, logSave){
        let dataString = JSON.stringify(inputData, null, 2);
        try {
            fs.writeFile(Path, dataString, (err) => {
                if (err) throw err;
                if (logSave) {
                    console.log('Data written to file');
                }
            });
            
        } 
        catch (error) {
            LOG.error(error, `Failed to write to file ${Path}`);
        }
    },

    getValidBotTag(message,text)
    {
        if (text.includes(`<@!${message.guild.me.id}>`)) {
            return (`<@!${message.guild.me.id}>`)
        }
        else if (text.includes(`<@${message.guild.me.id}>`)) {
            return (`<@${message.guild.me.id}>`)
        }
        else if (text.includes(`<@&${message.guild.me.id}>`)) {
            return (`<@&${message.guild.me.id}>`)
        }
        else{
            return false;
        }
    }
}

function isAllowedToSend(message){
    var memberPerms = message.channel.permissionsFor(message.guild.me);
    return memberPerms.has('SEND_MESSAGES')
}