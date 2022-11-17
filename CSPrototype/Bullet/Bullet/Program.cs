// Bullet Trajectory calculator in 3D vector Written by Jacob Thang for prototyping in Unity. 


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {



            var bullet1 = new Bullet(Bullet.dragModel.G7, 0.0097198359, 822, 10, 0.0078232, 0.0338328, 101325, 288.16);
            bullet1.fire(5);
            bullet1.calculateRetardation();

            Console.WriteLine("Stability Factor : " + bullet1.GetStabilityFactor());



        }
    }
}


class Bullet
{

    //G1 – G1 projectiles are flatbase bullets with 2 caliber nose ogive and are the most common type of bullet.
    //G2 – bullets in the G2 range are Aberdeen J projectiles
    //G5 – G5 bullets are short 7.5 degree boat-tails, with 6.19 caliber long tangent ogive
    //G6 – G6 are flatbase bullets with a 6 cailber secant ogive
    //G7 – Bullets with the G7 BC are long 7.5 degree boat-tails, with 10 caliber tangent ogive, and are very popular with manufacturers for extremely low-drag bullets.

    // Vectors
    double[] start_Pos = new double[3];
    double[] pos = new double[3];
    double[] spin = new double[3];
    double[] lateralAccel = new double[3];
    double[] drag = new double[3];
    double[] velocity = new double[3];
    double[] centripetal = new double[3];
    double[] coriolis = new double[3];
    double[] prev_coriolis = new double[3];
    double[] wind_Vector = new double[3];

    private double dt;
    private double g = -9.80665;
    private double mass;
    private double twist;
    private double muzzle_Velocity;
    private double bullet_Dia;
    private double bullet_Len;
    private double baro;
    private double pressure;
    private double temp;

    //Imperial 
    private double muzzle_Vel_Fps;
    private double grains;
    private double calibers;
    private double bullet_Dia_Inch;
    private double bullet_Len_Inch;
    private double temp_F;
    private double twist_Calibers;

    private double stability_Fac;

    public enum dragModel { G1, G2, G3, G4, G5, G6, G7 };
    public dragModel drag_Model;

    public Bullet(dragModel dModel , double mass, double muzzleVelocity, double barrelTwist, double bulletDia, double bulletLen, double pressure, double temp)
    {
        this.drag_Model = dModel;
        this.mass = mass;
        this.twist = barrelTwist;
        this.bullet_Dia = bulletDia;
        this.bullet_Len = bulletLen;
        this.temp = temp;
        this.muzzle_Velocity = muzzleVelocity;
        this.pressure = pressure;
        this.grains = this.mass * 15432.4;
        this.bullet_Dia_Inch = this.bullet_Dia * 39.3701;
        this.bullet_Len_Inch = this.bullet_Len * 39.3701;
        this.calibers = this.bullet_Len / this.bullet_Dia;
        this.twist_Calibers = barrelTwist / this.bullet_Dia_Inch;

        stability_Fac = GetStabilityFactor();


    }

    public void fire(double firing_angle)
    {
        this.pos[0] = 0;
        this.pos[1] = 0;
        this.pos[2] = 0;

        firing_angle = firing_angle * (Math.PI / 180);

        //Y
        this.velocity[1] = Math.Sin(firing_angle ) * this.muzzle_Velocity;
        //X
        this.velocity[0] = Math.Cos(firing_angle) * this.muzzle_Velocity;
        //Z
        this.velocity[2] = 0;

        Console.WriteLine("Velocity x = " + velocity[0] + " y = " + velocity[1] + " z = " + velocity[2]);



    }


    public void update()
    {



    }


    void getSpeed()
    {

        // Get Velocity Vector3 from unity. Using rb.Velocity.Magnitude
        this.velocity[0] = this.velocity[0] + this.wind_Vector[0] * dt;
        this.velocity[1] = this.velocity[1] + this.wind_Vector[1] * dt;
        this.velocity[2] = this.velocity[2] + this.wind_Vector[2] * dt;



    }

    public void calculateRetardation()
    {

        double velFps = Math.Sqrt( Math.Pow(this.velocity[0],2) + Math.Pow(this.velocity[1], 2) + Math.Pow(this.velocity[2], 2)) * 3.2808399 ;

        double A = -1;
        double M = -1;

        if (drag_Model == dragModel.G1)
        {
            if (velFps > 4230) { A = 1.477404177730177e-04; M = 1.9565; }
            else if (velFps > 3680) { A = 1.920339268755614e-04; M = 1.925; }
            else if (velFps > 3450) { A = 2.894751026819746e-04; M = 1.875; }
            else if (velFps > 3295) { A = 4.349905111115636e-04; M = 1.825; }
            else if (velFps > 3130) { A = 6.520421871892662e-04; M = 1.775; }
            else if (velFps > 2960) { A = 9.748073694078696e-04; M = 1.725; }
            else if (velFps > 2830) { A = 1.453721560187286e-03; M = 1.675; }
            else if (velFps > 2680) { A = 2.162887202930376e-03; M = 1.625; }
            else if (velFps > 2460) { A = 3.209559783129881e-03; M = 1.575; }
            else if (velFps > 2225) { A = 3.904368218691249e-03; M = 1.55; }
            else if (velFps > 2015) { A = 3.222942271262336e-03; M = 1.575; }
            else if (velFps > 1890) { A = 2.203329542297809e-03; M = 1.625; }
            else if (velFps > 1810) { A = 1.511001028891904e-03; M = 1.675; }
            else if (velFps > 1730) { A = 8.609957592468259e-04; M = 1.75; }
            else if (velFps > 1595) { A = 4.086146797305117e-04; M = 1.85; }
            else if (velFps > 1520) { A = 1.954473210037398e-04; M = 1.95; }
            else if (velFps > 1420) { A = 5.431896266462351e-05; M = 2.125; }
            else if (velFps > 1360) { A = 8.847742581674416e-06; M = 2.375; }
            else if (velFps > 1315) { A = 1.456922328720298e-06; M = 2.625; }
            else if (velFps > 1280) { A = 2.419485191895565e-07; M = 2.875; }
            else if (velFps > 1220) { A = 1.657956321067612e-08; M = 3.25; }
            else if (velFps > 1185) { A = 4.745469537157371e-10; M = 3.75; }
            else if (velFps > 1150) { A = 1.379746590025088e-11; M = 4.25; }
            else if (velFps > 1100) { A = 4.070157961147882e-13; M = 4.75; }
            else if (velFps > 1060) { A = 2.938236954847331e-14; M = 5.125; }
            else if (velFps > 1025) { A = 1.228597370774746e-14; M = 5.25; }
            else if (velFps > 980) { A = 2.916938264100495e-14; M = 5.125; }
            else if (velFps > 945) { A = 3.855099424807451e-13; M = 4.75; }
            else if (velFps > 905) { A = 1.185097045689854e-11; M = 4.25; }
            else if (velFps > 860) { A = 3.566129470974951e-10; M = 3.75; }
            else if (velFps > 810) { A = 1.045513263966272e-08; M = 3.25; }
            else if (velFps > 780) { A = 1.291159200846216e-07; M = 2.875; }
            else if (velFps > 750) { A = 6.824429329105383e-07; M = 2.625; }
            else if (velFps > 700) { A = 3.569169672385163e-06; M = 2.375; }
            else if (velFps > 640) { A = 1.839015095899579e-05; M = 2.125; }
            else if (velFps > 600) { A = 5.71117468873424e-05; M = 1.950; }
            else if (velFps > 550) { A = 9.226557091973427e-05; M = 1.875; }
            else if (velFps > 250) { A = 9.337991957131389e-05; M = 1.875; }
            else if (velFps > 100) { A = 7.225247327590413e-05; M = 1.925; }
            else if (velFps > 65) { A = 5.792684957074546e-05; M = 1.975; }
            else if (velFps > 0) { A = 5.206214107320588e-05; M = 2.000; }
        }

        if (drag_Model == dragModel.G2)
        {
            if (velFps > 1674) { A = .0079470052136733; M = 1.36999902851493; }
            else if (velFps > 1172) { A = 1.00419763721974e-03; M = 1.65392237010294; }
            else if (velFps > 1060) { A = 7.15571228255369e-23; M = 7.91913562392361; }
            else if (velFps > 949) { A = 1.39589807205091e-10; M = 3.81439537623717; }
            else if (velFps > 670) { A = 2.34364342818625e-04; M = 1.71869536324748; }
            else if (velFps > 335) { A = 1.77962438921838e-04; M = 1.76877550388679; }
            else if (velFps > 0) { A = 5.18033561289704e-05; M = 1.98160270524632; }
        }

        if (drag_Model == dragModel.G5)
        {
            if (velFps > 1730) { A = 7.24854775171929e-03; M = 1.41538574492812; }
            else if (velFps > 1228) { A = 3.50563361516117e-05; M = 2.13077307854948; }
            else if (velFps > 1116) { A = 1.84029481181151e-13; M = 4.81927320350395; }
            else if (velFps > 1004) { A = 1.34713064017409e-22; M = 7.8100555281422; }
            else if (velFps > 837) { A = 1.03965974081168e-07; M = 2.84204791809926; }
            else if (velFps > 335) { A = 1.09301593869823e-04; M = 1.81096361579504; }
            else if (velFps > 0) { A = 3.51963178524273e-05; M = 2.00477856801111; }
        }

        if (drag_Model == dragModel.G6)
        {
            if (velFps > 3236) { A = 0.0455384883480781; M = 1.15997674041274; }
            else if (velFps > 2065) { A = 7.167261849653769e-02; M = 1.10704436538885; }
            else if (velFps > 1311) { A = 1.66676386084348e-03; M = 1.60085100195952; }
            else if (velFps > 1144) { A = 1.01482730119215e-07; M = 2.9569674731838; }
            else if (velFps > 1004) { A = 4.31542773103552e-18; M = 6.34106317069757; }
            else if (velFps > 670) { A = 2.04835650496866e-05; M = 2.11688446325998; }
            else if (velFps > 0) { A = 7.50912466084823e-05; M = 1.92031057847052; }
        }

        if (drag_Model == dragModel.G7)
        {
            if (velFps > 4200) { A = 1.29081656775919e-09; M = 3.24121295355962; }
            else if (velFps > 3000) { A = 0.0171422231434847; M = 1.27907168025204; }
            else if (velFps > 1470) { A = 2.33355948302505e-03; M = 1.52693913274526; }
            else if (velFps > 1260) { A = 7.97592111627665e-04; M = 1.67688974440324; }
            else if (velFps > 1110) { A = 5.71086414289273e-12; M = 4.3212826264889; }
            else if (velFps > 960) { A = 3.02865108244904e-17; M = 5.99074203776707; }
            else if (velFps > 670) { A = 7.52285155782535e-06; M = 2.1738019851075; }
            else if (velFps > 540) { A = 1.31766281225189e-05; M = 2.08774690257991; }
            else if (velFps > 0) { A = 1.34504843776525e-05; M = 2.08702306738884; }
        }

        //if (drag_Model == dragModel.G8)
        //{
        //    if (velFps > 3571) { A = .0112263766252305; M = 1.33207346655961; }
        //    else if (velFps > 1841) { A = .0167252613732636; M = 1.28662041261785; }
        //    else if (velFps > 1120) { A = 2.20172456619625e-03; M = 1.55636358091189; }
        //    else if (velFps > 1088) { A = 2.0538037167098e-16; M = 5.80410776994789; }
        //    else if (velFps > 976) { A = 5.92182174254121e-12; M = 4.29275576134191; }
        //    else if (velFps > 0) { A = 4.3917343795117e-05; M = 1.99978116283334; }
        //}

       // if (A != -1 && M != -1 && velFps > 0 && velFps < 100000)
        //{
       //     retardation = A * Math.Pow(velFps, M) / ballisticCoefficient;
       //     retardation = retardation / 3.2808399;
        //}
    }



    public double GetStabilityFactor()
    {

        //Improved Miller Formula

        unitConversion();

        double stabilityFactor;

        stabilityFactor = 30 * this.grains / (Math.Pow(this.twist_Calibers, 2) * Math.Pow(this.bullet_Dia_Inch, 3) * this.calibers * (1 + Math.Pow(this.calibers, 2)));
        stabilityFactor = stabilityFactor * Math.Pow((this.muzzle_Vel_Fps / 2800f), (1/3));
        stabilityFactor = stabilityFactor * ((this.temp_F + 460) / (519)) * (29.92 / this.baro);

        return stabilityFactor;
    }

    private void unitConversion()
    {

        this.muzzle_Vel_Fps = this.muzzle_Velocity * 3.28084;
        this.baro = this.pressure * 0.0002953;
        this.temp_F = ((this.temp - 273.15) * 1.8) + 32;

    }









}