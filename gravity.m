function [xdot,ydot] = gravity(pos)

g = 9.8065; % Gravitational acceleration [m/s2]
me = 5.9722e24; % Constant mass of earth [kg]
re = 6356766 ; % Constant mass of earth [m]

xdot = -g * me * (pos(1)/ (re + pos(1)^3));
ydot = -g * me * (re+ pos(1)/ (re + pos(2)^3));

end

