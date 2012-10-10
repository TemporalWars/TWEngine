#region File Description
//-----------------------------------------------------------------------------
// Sounds.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Audio.Enums
{
    // 6/10/2012 - Updated Enum with number ranges.
    /// <summary>
    /// The <see cref="Sounds"/> Enum contains all the sounds for the game.
    /// </summary>
    /// <remarks>
    /// (6/10/2012): Improvment: The sounds are now given group numbers, which identify which 'SoundBank' the sounds belong to.  
    /// This eliminates the need to have the user call the AudioManager with a 'SoundBank', and removes errors which can occur from
    /// calling the AudioManager with a wrong 'SoundBank' to 'Sound' combination.
    /// </remarks>
    public enum Sounds
    {
// ReSharper disable InconsistentNaming
#pragma warning disable 1591

        // 2/7/2011
        #region AmbientMusic (Range 0 - 99 = SoundBank "AmbientMusic")

        //AltDream,
        //CornerPocket,
        //Atmospheric_DarkMatter,
        ComeAndPlay = 0,
        KickItBack = 1,
        LetterOfIntent = 2,
        Mystique = 3,
        RiverFlow = 4,
        // Note (6/27/2012) - New Songs from Camtasia Studio 8.
        AcousticCadence = 5,
        Amity = 6,
        AurinBee = 7,
        BackyardSafari = 8,
        BareKnuckles = 9,
        BluesCountry = 10,
        CoastToCoast = 11,
        Electropulse = 12,
        LaTiDa = 13,
        PianoLounge = 14,
        RondeauQuartet = 15,
        SeniorYear = 16,
        SledgeHammer = 17,
        ThemePark = 18,
        Undefeated = 19,

        #endregion

        #region Ambient (Range 100 - 199 = SoundBank "Ambient")

        Birds_Creepy = 100,
        Birds_Crows = 101,
        Birds_Desert = 102,
        Birds_Falcons = 103,
        Birds_Mountain = 104,
        Birds_Owls = 105,
        Birds_Roosters = 106,
        Birds_Seagulls = 107,
        Birds_Typical = 108,
        Crick_Group = 109,
        Crick_Single = 110,
        Critters_Desert = 111,
        Rain1 = 112,
        Rain2 = 113,
        Rain3 = 114,
        Rain4 = 115,
        WF_Selected = 116,
        Wind_ColdGroup = 117,
        Wind_GrassGroup = 118,
        Wind_MtnGroup = 119,

        #endregion

        #region Explosions (Range 200 - 299 = SoundBank "Explosions")

        Exp_BomberGroup = 200,
        Exp_C4Group = 201,
        Exp_Harsh_Big1 = 202,
        Exp_Harsh_Big2 = 203,
        Exp_Harsh_Big3 = 204,
        Exp_Harsh_Big4 = 205,
        Exp_Harsh_Big5 = 206,
        Exp_Harsh_Big6 = 207,
        Exp_Harsh_Small1 = 208,
        Exp_Harsh_Small2 = 209,
        Exp_Harsh_Small3 = 210,
        Exp_Harsh_Small4 = 211,
        Exp_Harsh_Small5 = 212,
        Exp_Medium1 = 213,
        Exp_Medium2 = 214,
        Exp_Medium3 = 215,
        Exp_Medium4 = 216,
        Exp_Medium5 = 217,
        Exp_Medium6 = 218,
        Exp_Medium7 = 219,
        Exp_Medium8 = 220,
        Exp_Medium9 = 221,
        Exp_Medium10 = 222,
        Exp_Medium11 = 223,
        Exp_Medium12 = 224,
        Exp_Medium13 = 225,
        Exp_MediumGroup = 226,
        Exp_RocketGroup = 227,       
        Exp_Smooth1 = 228,
        Exp_Smooth2 = 229,
        Exp_Smooth3 = 230,
        Exp_Smooth4 = 231,
        Exp_Smooth5 = 232,
        Exp_Smooth6 = 233,
        Exp_SmoothGroup = 234,

        #endregion

        #region Interface (Range 300 - 399 = SoundBank "Interface")

        Cash_Down = 301,
        Cash_Up = 302,
        Menu_Click = 303,        

        #endregion

        #region Mechanical (Range 400 - 499 = SoundBank "Mechanical")

        ChopperIdleLoop1 = 401,
        ChopperIdleLoop2 = 402,
        ChopperIdleLoop3 = 403,
        TankMove = 404,

        #endregion

        #region SciFiWeapons (Range 500 - 599 = SoundBank "SciFiWeapons")

        Cannon1 = 500,
        Cannon2 = 501,
        Cannon3 = 502,
        ElectroGun1 = 503,
        ElectroGun2 = 504,
        ElectroGun3 = 505,
        ElectroGun4 = 506,
        ElectroGun5 = 507,
        ElectroGun6 = 508,
        GuardGun_Group = 509,
        GunshotWReload_Group = 510,
        Laser1 = 511,
        Laser2 = 512,
        Laser3 = 513,
        Laser4 = 514,
        Laser5 = 515,
        Laser6 = 516,
        LaserMissle_Group = 517,
        MachineGunA_Group = 518,
        MachineGunB_Group = 519,
        MachineGunC = 520,
        PowerDown1 = 521,
        PowerDown2 = 522,
        PowerDown3 = 523,
        PowerDown4 = 524,
        PowerUp1 = 525,
        PowerUp2 = 526,
        PowerUp3 = 527,
        PowerUp4 = 528,
        PulseGun1 = 529,
        PulseGun2 = 530,
        PulseGun3 = 531,
        PulseGun4 = 532,
        PulseGun4a = 533,
        PulseGun5 = 534,
        RocketFire_Group = 535,
        RocketMissle_Group = 536,
        SuperGun1 = 537,
        SuperGun2 = 538,
        WeaponsReload = 539,

        #endregion

#pragma warning restore 1591
// ReSharper restore InconsistentNaming
    }
}