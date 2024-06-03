# NeoFPS_Rayfire
NeoFPS and Rayfire for Unity integration assets 

## Requirements

This repository was created using Unity 2020.3

It requires the assets [NeoFPS](https://assetstore.unity.com/packages/templates/systems/neofps-150179?aid=1011l58Ft) and [Rayfire for Unity](https://assetstore.unity.com/packages/tools/game-toolkits/rayfire-for-unity-148690?aid=1011l58Ft).

## Installation

This integration example is intended to be dropped in to a fresh project along with NeoFPS and Rayfire.

1. Import NeoFPS and apply the required Unity settings using the NeoFPS Settings Wizard. You can find more information about this process [here](https://docs.neofps.com/manual/neofps-installation.html).

2. Import the Rayfire for Unity asset.

3. Clone this repository to a folder inside the project Assets folder such as "NeoFPS_Rayfire"

4. Create a **Rayfire** layer in the project's layer settings (*Project Settings/Tags and Layers*). Any rayfire objects should be placed on this layer so that they can be treated separately to regular physics objects. **In the demo, the rayfire layer is set to layer 31**.

5. Modify the physics collision matrix so that the rayfire layer **does not** collide with the following layers:
+ *EnvironmentDetail*
+ *CharacterFirstPerson*
+ *CharacterNonColliding*

You can also optimise further, but the most important layers that rayfire objects should collide with are:
+ *Default*
+ *EnvironmentRough*
+ *CharacterControllers*
+ *DynamicProps*
+ *Doors*
+ *Rayfire*

## Features

The integration focusses on 2 areas: firearms and the FPS character controller.

### Firearms

The **Rayfire Bullet Ammo Effect** component is an ammo effect module for the NeoFPS modular firearm system. This adds the ability to activate and destroy rayfire rigid objects to any NeoFPS firearm and can be stacked with bullet effects such as penetration and ricochet. For more information on modular firearms. see the [NeoFPS Documentation](https://docs.neofps.com/manual/weapons-modular-firearms.html).

The **Rayfire Pooled Explosion** component is a specialisation of the NeoFPS explosion component which adds the ability to activate and destroy rayfire rigid objects within its explosion radius. You can swap the **Pooled Explosion** component from any explosion prefabs with this, and then use it in explosive weapons and environmental effects such as exploding barrels.

### Character Controller

The **Rayfire Character Activator** component acts as an activation zone that fits to the character controller's capsule collider to activate rayfire rigid objects in its vicinity. It should be placed on a child object of the character prefab that uses the *Default* layer. The component will also add a capsule collider, and set it to a trigger at runtime. You can set the thickness of the activator area (how far it extends beyond the character capsule), and the delay from detecting a rayfire rigid object to activating it.

The **Rayfire Motion Controller Addon** component is added to the root character object alongside its motion controller component. It adds the ability to disable collisions with rayfire rigid objects. An example of when this could be used is walking on thin ice. When you want the character to fall through, disable collisions and activate the **Rayfire Character Activator**. The object will then crumble as you fall through.

You can also activate a *wrecking ball* state for the character. This allows you to specify a threshold speed for any collisions that will cause an explosive effect for nearby rayfire rigid objects, along with the radius and offset for the effect. A positive offset will push objects away from the collision (useful for shoulder barges and similar movements), while a negative offset will push them back past the character (useful for effects like ground slams where you want the ground to lift). As soon as the character wrecking ball effect fires, the character leaves that state and it must be manually activated again.

All of the above character controller effects can be activated through trigger zones, motion graph behaviours, or accessing their API. The **RayfireCharacterTriggerZone** can be added to an object on the *TriggerZones* layer with a collider set with *Is Trigger* as true. It has options for each of the above effects.

The motion graph behaviours: **Rayfire Character Activator**, **Rayfire Character Collision** and **Rayfire Character Wrecking Ball** can be added to motion graph states and subgraphs to control when the effects are active. Each has options for entering and exiting the state, so if you want the effect to persist across multiple states then you can enable it in one and then disable it in another. For example, the demo motion graph enables the wrecking ball effect on entering the ground slam state, and then disables it when entering the grounded state (the wrecking ball is triggered immediately on collisions, while the character won't count as grounded until the next frame).

## Demo

The demo scene provides a number of example rayfire setups that might be used in a game, along with a character (motion graph) and some weapons that are set up to interact with them.

The motion graph is set up with a jetpack (hold space / jump to hover), ground dash (alt / ability key while on the ground) and ground slam (alt / ability key while in the air). The ground dash and ground slam use the wrecking ball feature to slam through rayfire objects. The paving slabs and buildings are there mainly to give you things to slam into.

The bridges each have a trigger zone set up that enables the character activator. Running over the bridges will cause them to collapse after a brief delay.

## Troubleshooting & Known Issues

- You will need to modify any cameras to make the **Rayfire** layer visible. This may be off by default
- If your debris colliders have bevels then you may need to increase the **NeoCharacterController**'s "Ground Hit Lookahead" setting. This allows the character to check whether it hit a slope or the corner of a ledge, and a bevel may trick the controller into thinking it is a slope when it is not. The demo character uses a setting of 0.1 compared to the default 0.005
- Due to the way that the NeoCharacterController acts on physics objects (it is pushed by and exerts a force on all non-kinematic rigidbodies as it moves), I found that the best setup for Rayfire clusters was to set their simulation type as *Kinematic* and then activate them via the integration systems. If the rayfire rigid is set to use the *Dynamic*, *Sleeping* or *Inactive* simulation types, then you can slowly push through the objects just by holding down the movement direction into their surface.
- Very occasionally the wrecking ball will not trigger the objects to destruct, so it will just stop the character dead.
- If you shoot out the two sides of the stone bridge so that the center drops as a single section, then walking over that will add a huge upward force to the character controller.