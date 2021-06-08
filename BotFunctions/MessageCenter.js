//This file is dedicated to taking all messages and sorting them to the according functions accross the BOT. ALL messages go through here. This was created to clean up Index.js
//Message Center works by takin in the input, figuring out where it needs to go, and collecting up any responses, so a single large message can be sent rather than many small ones
//in short, its an central IO hub between the end user and the BOT

// -----------------------------------------------------------------------------------------------------------------------------------------------------------

//code outside of functions only runs at init (once).
const Discord = require("discord.js");
const BOT = require("../index.js"); //import from index
const COLLECTOR = require('./MessageCollector.js');
const CONFIG = require('../config.json');
const LOG = require('./Log.js');
const fs = require('fs');
const Utils = require("./Utils.js");

var UserCommandList = [];
BOT.commands = new Discord.Collection(); //creates list of allowed commands based on folder content.
const UserCommandFiles = fs.readdirSync(`./Commands/User/`).filter(file => file.endsWith('.js'));

for(const filename of UserCommandFiles){
    const command = require (`../Commands/User/${filename}`); 
    BOT.commands.set(command.name,command);
    UserCommandList.push(command.name);
}

var AdminCommandList = [];
const AdminCommandFiles = fs.readdirSync(`./Commands/Admin`).filter(file => file.endsWith('.js'));

for(const filename of AdminCommandFiles){
    const command = require (`../Commands/Admin/${filename}`); 
    BOT.commands.set(command.name,command);
    AdminCommandList.push(command.name);
}

var PassiveCommandList = [];
const PassiveCommandFiles = fs.readdirSync(`./Commands/Passive`).filter(file => file.endsWith('.js'));

for(const filename of PassiveCommandFiles){
    const command = require (`../Commands/Passive/${filename}`); 
    BOT.commands.set(command.name,command);
    PassiveCommandList.push(command.name);
}

module.exports = {
    messageHandler(message) {//First split for messages
        var hasPrefix = false;
        if (message.content.startsWith(CONFIG.Prefix)) {
            hasPrefix = true;
        }
        var args = messagePrep(message,hasPrefix);
        if(UserCommandList.includes(args[0]) && hasPrefix){
            return RunUserFunctions(message,args);
        }
        else if (AdminCommandList.includes(args[0]) && hasPrefix){
            return RunAdminFunctions(message,args)
        }
        else{
            if (PassiveCommandList.length > 1){
                var promises = []
                PassiveCommandList.forEach(element => {
                    promises.push(BOT.commands.get(element).execute(message,args));
                });
                return Promise.all(promises);
            }
            else if(PassiveCommandList.length = 1){
                return BOT.commands.get(PassiveCommandList[0]).execute(message,args);//attempt to run a given command. if it exists
            }
            else{
                return;
            }
        }
    }
}

function messagePrep(message){
    let args = message.content.substring(CONFIG.Prefix.length).split(" ");
    for (i = 0; i<args.length;i++){//all lowercase for user compatibility
        args[i] = args[i].toLowerCase();
    }

    if (args.length > 1) {
        args = Utils.CleanArray(args)        
    }
    return args;
}

function RunUserFunctions(message,args){
    LOG.info(`New message!\n- User: ${message.author.username} \n- Server: ${message.guild.name} (${message.guild.id})\n- Command: ${message.content}`);

    try {
        return BOT.commands.get(args[0]).execute(message,args);//attempt to run a given command. if it exists
    }
    catch(err) {//failover if a command doesnt exist or an error occours
        COLLECTOR.Clear();
        LOG.error('',err);
        COLLECTOR.Add("Something went wrong while processing your command. You should not be seeing this message. If you want to help solve the issiue, take a screenshot of your interaction with the BOT and contact an Admin.");
    }
}

function RunAdminFunctions(message,args){
    if(!CONFIG.Admins.includes(message.author.id)) return;

    LOG.info(`New ADMIN command!\n- User: ${message.author.username} \n- Server: ${message.guild.name}\n- Command: ${message.content}`,'AE00FF');

    try {
        return BOT.commands.get(args[0]).execute(message,args);//attempt to run a given command. if it exists
    }
    catch(err) {//failover if a command doesnt exist or an error occours
        COLLECTOR.Clear();
        LOG.error('',err);
        COLLECTOR.Add("Something went wrong while processing your command. You should not be seeing this message. If you want to help solve the issiue, take a screenshot of your interaction with the BOT and contact an Admin.");
    }
}