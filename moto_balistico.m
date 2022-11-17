function dY=moto_balistico(t,Y)

global rho S cd m g opz_aereo_y

%X(1)=Vx
%X(2)=Vy
%X(3)=x
%X(6)=y;

Vx=Y(1); 
Vy=Y(2);

% Moto lungo x
% Sommatorie forze lungo x==> Solo aereo
Faereo_x=0.5*rho*S*Vx^2*cd;

% Moto lungo y
% Sommatorie forze lungo y==> Aereo+Gravità
Faereo_y=0.5*rho*S*Vy^2*cd*opz_aereo_y;
Fgrav=m*g;

dY(1)=-Faereo_x/m;                 
dY(2)=-(Faereo_y*sign(Vy)+Fgrav)/m;
dY(3)=Vx;
dY(4)=Vy;
dY=dY';



