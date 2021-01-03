const { User } = require('discord.js');
const CONFIG = require('../config.json');
const PREFIX = CONFIG.Prefix

module.exports = {
    shouldRespond(message){
        if (message.author.id == message.guild.me.id) return false;
        if (!isAllowedToSend(message)) return false; //doesnt respond if not allowed to send
        if (message.content.includes(message.guild.me.id)) return true;
        if (!message.content[0] === PREFIX) return false;
        if (message.author.bot) return false;
        if (message.content.length <= 1) return false;
        if (message.content.includes('https://')) return false;
        if (message.content.includes('www.')) return false;
        return true;
    }
}

function isAllowedToSend(message){
    var memberPerms = message.channel.permissionsFor(message.guild.me);
    return memberPerms.has('SEND_MESSAGES')
}