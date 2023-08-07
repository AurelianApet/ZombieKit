

Activate the disabled TransData game object from the Map01 scene to play the battle map directly, without going through the normal process - hometown/load/launch battle, etc.

Do not leave it activated in the final version because the TransData from the Game scene will become a duplicate - it has DontDestroyOnLoad(this) so it can be carried back and forth between the battle map and hometown.





