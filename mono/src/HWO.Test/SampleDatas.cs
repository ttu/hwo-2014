﻿namespace HWO.Test
{
    public static class SampleDatas
    {
        public static string YourCarOwn = @"{'msgType':'yourCar','data':{'name':'Bit-Bil','color':'red'},'gameId':'b4356d76-8c85-4f01-b79d-c4b642631080'}";

        public static string GameInitKeimola = @"{'msgType':'gameInit','data':{'race':{'track':{'id':'keimola','name':'Keimola','pieces':[{'length':100.0},{'length':100.0},{'length':100.0},{'length':100.0,'switch':true},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':200,'angle':22.5,'switch':true},{'length':100.0},{'length':100.0},{'radius':200,'angle':-22.5},{'length':100.0},{'length':100.0,'switch':true},{'radius':100,'angle':-45.0},{'radius':100,'angle':-45.0},{'radius':100,'angle':-45.0},{'radius':100,'angle':-45.0},{'length':100.0,'switch':true},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':200,'angle':22.5},{'radius':200,'angle':-22.5},{'length':100.0,'switch':true},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'length':62.0},{'radius':100,'angle':-45.0,'switch':true},{'radius':100,'angle':-45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'radius':100,'angle':45.0},{'length':100.0,'switch':true},{'length':100.0},{'length':100.0},{'length':100.0},{'length':90.0}],'lanes':[{'distanceFromCenter':-10,'index':0},{'distanceFromCenter':10,'index':1}],'startingPoint':{'position':{'x':-300.0,'y':-44.0},'angle':90.0}},'cars':[{'id':{'name':'Bit-Bil','color':'red'},'dimensions':{'length':40.0,'width':20.0,'guideFlagPosition':10.0}}],'raceSession':{'laps':3,'maxLapTimeMs':60000,'quickRace':true}}},'gameId':'21c1a160-0b0a-4cc9-9524-ffccc1114a54'}";

        public static string MyCarPositions = @"{'msgType':'carPositions','data':[{'id':{'name':'Bit-Bil','color':'red'},'angle':0.0,'piecePosition':{'pieceIndex':0,'inPieceDistance':0.0,'lane':{'startLaneIndex':0,'endLaneIndex':0},'lap':0}}],'gameId':'e7a68185-3d1e-45c9-af18-05f1704e1f58'}";
    }
}