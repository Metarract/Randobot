# v1.0.3
- Fixed issue with unmapped commands not being properly handled by `GetCommandMapping`
  - Was improperly assuming the default value of a string, doing a proper null check now
- Added some error handling around the command handler so that commands don't necessarily crash the bot
  - Mistakenly thought that errors would bubble up to `Program` but will probably have to do some better handling of errors within each event if it makes sense

# v1.0.2
- Added example Data Directory to publish
- Fixed even more issues with redirect, this time with string concatenation

# v1.0.1
- Fixed an issue with redirect, it was not working before and now it should be


# v1.0.0
A little hefty, but she should work! This one is for Windows 64bit specifically

What we got:

- Simple command bot for twitch with a single command to output a randomized string from local lists, constructed via a configurable template string
- Command Mapping where internal commands can have other words mapped to them
- Simple local API to handle initial token creation / authorization
- Refreshes and rotates tokens as needed
- Logging (through NLog). Defaults to logging everything to ./logs/log.txt, archives once per day / @ size, max 30 logs
- Check README for details on setup