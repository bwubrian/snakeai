# Using Reinforcement Learning to play the Snake game

Trained with Unity's implentation of [PPO](https://openai.com/blog/openai-baselines-ppo/) on various board sizes.

Shown are nine independent parallel randomized instantiations on an 8x8 board.

[![Alt text](https://img.youtube.com/vi/clKv5U-Cvdc/0.jpg)](https://www.youtube.com/watch?v=clKv5U-Cvdc)

| Block Color  | Game Object |
| ------------- | ------------- |
| Red  | Snake Head  |
| White  | Snake Body  |
| Yellow  | Food Pellet  |

Steps are incremented each time the agent takes an action. Score is incremented each time the snake eats a food pellet 
and increases in length by one. The current model is on average able to consistently acheive a score around half the total size 
of the board (e.g. length of 32 for board size of 8x8) but rarely gets far above that.

