clear all
close all
clc

% Written by Jacob Tang Drag only
% Trajectory prediction for 5.56 round.

global rho S cd m g opz_aereo_y

rho=1.225; % Air density [kg/m3]
S=0.00000635614; % Surface [m2]
cd=0.280; % Drag coefficient [-]
m=0.062; % mass [kg]
g=9.8065; % gravitational acceleration [m/s2]
tf=30; % Simulation final time [s]

% Initial conditions
V0=993; % Initial speed [m/s]
x0=0; % Initial x [m]
y0=100; % Initial y [m]

opz_aereo_y=1; %Consider or not the aerodynamic drag along y

alpha_ini=0:2:10; %Launch angle

%Some figure setting and mtrix initialization
col='brgmck';
col=repmat(col,1,10);
range=zeros(size(alpha_ini));
max_h=range;
leg=cell(length(alpha_ini),1);

figure(1)
clf
subplot(2,2,[1 3])
hold on
grid on
xlabel('range [m]','FontWeight','bold')
ylabel('Height [m]','FontWeight','bold')
set(gca,'FontWeight','bold')

%Loop over launch angle
for ii=1:length(alpha_ini)
    alpha0=deg2rad(alpha_ini(ii));
    
    Vx0=V0*cos(alpha0);
    Vy0=V0*sin(alpha0);
    
    [t, Y]=ode45(@moto_balistico,[0 tf],[Vx0 Vy0 x0 y0]');
    
    x=linspace(min(Y(:,3)),max(Y(:,3)),1000);
    data=zeros(1000,3);
    
    for jj=1:4
        data(:,jj)=interp1(Y(:,3),Y(:,jj),x,'linear');
    end
    t=interp1(Y(:,3),t,x,'linear');
    
    pos_ground=find(data(:,4)<0,1,'first');
    data(pos_ground:end,:)=[];
    
    range(ii)=data(end,3);
    max_h(ii)=max(data(:,4));
    
    subplot(2,2,[1 3])
    plot(data(:,3),data(:,4),col(ii),'LineWidth',2)
    drawnow
    
    leg{ii}=['\alpha = ',num2str(alpha_ini(ii)),' °'];
end

subplot(2,2,[1 3])
legend(leg);

aa(1)=subplot(222);
plot(alpha_ini,range,'-o','LineWidth',2)
xlabel('\alpha [°]','FontWeight','bold')
ylabel('Range [m]','FontWeight','bold')
grid on
set(gca,'FontWeight','bold')
aa(2)=subplot(224);
plot(alpha_ini,max_h,'-o','LineWidth',2)
xlabel('\alpha [°]','FontWeight','bold')
ylabel('Max H [m]','FontWeight','bold')
set(gca,'FontWeight','bold')
grid on
linkaxes(aa,'x')