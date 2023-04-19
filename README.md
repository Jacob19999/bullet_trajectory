# bullet_trajectory

C# Script for the use in game engines that simulated a 3D virtual bullet's physical trajectory. Bullet class script contains the following features (Allows the user to initialize any bullet of any caliber . This is due to the implementation of drag model (G1, G2, G5, G6, G7, G8, GS) used by the US Army testing data) . The scrpt implements almost all aspects of bullet physics ( Gravity , Drag , Spin Drift , Coriolis , Centripetal , Wind ) and taking into account of (Firing angle , temperature , pressure , rifle twist , firing azimuth north south referenced , and latitude ). 

As of current, the script would be able to simulate a bullet trajectory to +- 5% MOA at 1000 yards compared to published data.

Addational area of implementation (WIP):
Yawing moment damping factor which affects bullets in close ranges.
Energy conservation (For bullet penetration)
Incidence angles (Richiots ricochet)
Bullet shatter / fragmentation




