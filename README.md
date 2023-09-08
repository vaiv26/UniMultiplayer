# UniMultiplayer
Prototype Version of Unity Multiplayer Game

## Prototype Description:
The prototype is divided into two scenes

* Scene 0: Sample Scene: Consists of UI to start the Game
* Scene 1: Game Scene: Consists of the systems, players and environment.

Playing the prototype: Start for scene 0, create room in the master client and click the find room on the client. A box will appear with all the available rooms. Select the desired room and start the game from the master Client. Once the game starts all the players will be transported automatically transported to scene 1: Game Scene. 

## Game Functionalities:

### Player System:
* The player has 100 health points. Getting health points to zero - kills the player.
* Upon death, a player respawns at a random spawn point scattered across the map.
* When one player kills another, they earn a point.
* The game ends when a player reaches 10 points.

### Weapon System
There are 3 weapon types:
* Pistol - fires an instant-hit raycast shot.
* Bow - fires a projectile with moderate flight speed and a small collider. The projectile is affected by gravity.
* Rocket launcher - fires a projectile with slow flight speed, a medium-sized collider, and a large blast radius upon impact.

### Additional Systems
* Each weapon has its own ammunition.
* The amount of ammunition in the weapon upon pickup equals the half of maximum ammunition capacity
* Upon player spawn, it is equipped with a random weapon.
* Weapons appear at weapon spawn points. After being picked up, a weapon disappears, and the subsequent one spawns at the same point after a specific interval.

### Items Systems: The Item List:
* Health packs - restore health upon pickup.
* Ammunition for each weapon.Items appear at item spawn points. Similar to weapons, they reappear after a specific time interval upon pickup.

### Spawners Systems: Spawner - is an object on a map, that spawns a specific item or weapon.

## Coding Architecture: In the Game Scene
* Room Manager Script handles the connection and disconnection
* PlayerManager Script handles the death and respawn of the player as well as counts the kills of each player
* Player Controller Script handles the controls all the controls of the player.
* [The items in the game follow an Architecture](https://drive.google.com/file/d/13oAmiaNpTOUJ6QPyH7SS9HI-DEHpPxSO/view?usp=sharing)

  
