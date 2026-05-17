# Blog Post 6: Final Showcase and Conclusion

![Final main menu](images/finalMainMenu.png)

The final version of **Stick Fight Arena** is a local 2D fighting/platform game where two players fight in small arenas using movement, attacks, and randomly spawning weapons. The game started as a simple prototype with two basic characters, but it ended up with a complete match flow, several arenas, weapons, animations, audio, menus, and a playable Web build.

The core gameplay is based on short rounds. Both players spawn into an arena, wait for the “Ready/Fight” countdown, and then try to defeat each other. A player can win a round by reducing the opponent’s health to zero or by knocking them out of the level. The score is then updated, and the next round begins. The game also has match settings, so players can choose between endless mode or modes like first to 1, first to 3, first to 5, and first to 10.

![Final gameplay](images/finalGameplay.png)

The final game includes movement mechanics such as running, jumping, double jumping, dashing, wall sliding, and ledge climbing. These mechanics make the fights feel more active and give players more ways to recover or escape. The combat system includes basic melee attacks, a knife, a gun, damage, knockback, health, and death logic. Weapons spawn during the round, which encourages players to move around the arena instead of staying in one place.

A lot of the final polish came from visuals, animations, and audio. I used stickman animations for idle, running, jumping, falling, attacking, shooting, dashing, wall sliding, ledge climbing, and death. I also added colorful 2D backgrounds for the arenas and used a custom arcade-style main menu. Sound effects were added for movement, weapons, impacts, round events, and menu navigation. These small details made the game feel much more responsive and alive.

![Pause menu](images/finalPauseMenu.png)

The UI was also an important part of the final version. The game now has a main menu, settings screen, pause menu, score display, health bars, and round messages. The pause menu allows the player to resume, restart the match, or return to the main menu. This made the game feel much more complete compared to the earlier version where everything started directly in the scene.

Overall, I am happy with the final result. The game is not perfect, and some things could still be improved, such as weapon alignment during certain animations, better balancing, more weapons, more arenas, and better arcade input testing. However, the important systems are working, and the game is fully playable from menu to match ending.

This project helped me work with many parts of Unity, including scripting, physics, animation, UI, audio, prefabs, Web builds, and game architecture. The most difficult part was not one single feature, but making all the systems work together without breaking the game loop. In the end, **Stick Fight Arena** became a complete small arcade fighting game, which was the main goal of the project.
