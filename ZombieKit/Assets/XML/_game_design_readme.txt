<!-- 

/*

GUIDE





Population types

There are 2 kind of population
- Building population
- Units population

***Building***
Every building max population is indipendent from each other!
The max population of every building is dependent on player level.

-> cap_x_prereq shows the level needed to create x number of a type of buildings

For example 
GoldForge.cap_3_prereq == 9 -> this means that the player has to reach level 9 before creating the third gold forge.



***Units***
The units (artifacts and summonables) are related to the SAME population, maxed based on buildings.
It starts at 0.

-> unit_max_pop = 0
-> current_pop = 0


 Population cap

There are 2 buildings that add population, one of them is the basic building that already exist in each game (the Wizard Academy)
These buildings are the only buildings with a value on these columns "pop_bonus", "lev_2_pop_bonus", lev_3_pop_bonus

pop_bonus is the bonus that grants the creation of a level 1 building
-> unit_max_pop += building.pop_bonus()

lev_2_pop_bonus and lev_3_pop_bonus are the bonus granted by the upgrade of the building to level 2 and 3
-> unit_max_pop += building.lev_2_pop_bonus()


 Current population

Every units has a weight that count toward the reach of the max population.
This value is stored under the column "pop_unit_weight"
current_pop += unit.pop_unit_weight()

If the player has too many units he couldn't create any other ones until he free some space or until he creates new buildings to allow more population.
-> if (current_pop >= unit_max_pop)
	then -> player can't more units


 IMPORTANT (gold_based and resource_cost columns)

This is important because the name is not friendly. But it's a very easy concept.
Every object can be bought spending Gold OR EITHER Mana Juice.
There's no object that needs both of them.

-> If the gold_based cell is "True" it means that the object may be bought spending Gold.
-> If the gold_based cell is "False" it means that the object may be bought spending Mana Juice.

Ex. 
***Creation of a building
Research Globe 		-> gold_based == True && resource_cost == 10000 	-> means that the player have to spend 10000 Gold to build a Research Globe 

***Upgrade of a building
The same is for the upgrade.
Summoning Circle 	-> gold_based == False && 2_upgrade_cost == 12500	-> means that the player have to spend 12500 Mana Juice to upgrade the Summoning Circle to lev 2


 IMPORTANT (sell_rate)

This is important because there's no column on the spreadsheet talking about this.
Sell rate always equal 60% building value and this value has to be added to the object.


object.sell_rate = object.Calculate_Value() * .6


 IMPORTANT Production buildings

You can see that one the production building are created using the opposite resource. This is inteded, but it may create confusion.
Gold Forge 	-> gold_based == False	-> the Gold Forge is built using Mana Juice, but produces Gold
Mana Generator	-> gold_based == True 	-> the Mana Generator is built using Gold, but produces Mana Juice

Production Rate
The resource generator buildings generates an amount of resources every second. To calculate the entire generation of a player we have to sum every produtcion.

-> player.goldGenerationPerSec = CalculateTotalGoldGeneration (building_on_gameboard, building_level)
Same for the Mana Juice

 Production Storage
Resources don't grow endlessly.
The player have to access to collect its resource before it reaches the cap.

-> player.maxGold = CalculateGoldStorage (buildings_on_gameboard, building_level)
Same for the Mana Juice


If the total exceed the max the resources won't generate more income.
-> if player.currentGold >= player.maxGold
		player.goldGenerationPerSec = 0

IMPORTANT Dobbit Toolhouse (object id = 12)
The Dobbit toolhouse also the only building that may be built independently.
For every other building the player could upgrade or create only one building at a time for each builder he has. 
(Ex. if the player has 3 dobbit toolhouse he may create one building while upgrading two others.) 
It's the same rule used by clash of clans builder's hut.

--------------------
Each of the other columns should become a the parameters to be added to the object class constructor.

*id* 					(string) 	is the object id
*name* 					(string) 	is the object name that will appear on screen if the object is selected
*description* 			(string) 	the description of the object
*type*					(type) 		the type of the object
*builder_pop*			(int) 		the number of buildings that could be built or upgraded at the same time
Only the first builder can be bought with soft currency, the other have to be bought spending crystals

*pop_bonus*				(int)		POPULATION BUILDINGS - the number to be added to the max population
*production_per_sec*	(float?)	PRODUCTION BUILDINGS - the amount of the resource generated per second
*store_capacity*		(int)		PRODUCTION & STORAGE BUILDINGS - the total amount of resource storage added by that building
*max_cap*				(int)		BUILDINGS - max number of building allowed
*pop_unit_weight*		(int)		UNITS - how many population slot occupies the unit
*time*					(string)	the time (in minutes) needed to create the building
*xp_award*				(int)		XP awarded for the completion of an object
*gold_based*			(bool)		To check which resource is needed to complete this kind of building
*resource_cost*			(int)		The quantity of the resource to create the building
*store_category*		(store_type)	The category where to put the object on the store
*obj_prereq*			(string)	The object that the player have to create before he can create this object
*cap_x_prereq*			(int)		BUILDINGS - This is the level needed to create x building type (x is a number from 1 to 5.)
*x_upgrade_lev_req*		(int)		BUILDINGS - This is the level required to upgrade a type of building (x is the level of the building)
*x_upgrade_obj_req*		(string)	BUILDINGS - These are the objects required to upgrade a type of building (x is the level of the building)
*x_upgrade_cost*		(int)		BUILDINGS - This is the amount of resources to be spent to upgrade the building
*x_upgrade_time*		(int)		BUILDINGS - This is the amount of time (in minutes) that have to pass to upgrade the building
*x_upgrade_xp_award*	(int)		BUILDINGS - XP awarded for the upgrade of an object
*lev_x_store_capacity*	(int)		PRODUCTION & STORAGE BUILDINGS the store capacity of the upgraded building (its not cumulative with the previous levels)
*lev_x_production*		(int)		PRODUCTION BUILDINGS - the amount of the resource generated per second of the upgraded building (its not cumulative with the previous levels)
*lev_x_pop_bonus*		(int)		POPULATION BUILDINGS - the number to be added to the max population granted by the upgraded building (its not cumulative with the previous levels)
*grid_size*				(int)		BUILDINGS - every buildings is squared, so we have just one value. If the valus is 3 the building is 3x3. If for then 4x4 and so on.
*object_graphic*		(string)	The graphic that appear on the screen
*store_graphic*			(string)	The graphic that appear on the UI store
*/

-->