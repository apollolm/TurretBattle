#pragma strict

static var scoreText:String = "TotalScore: ";

static var score = 0;

var _label : GameObject;

function Start () {
  _label = GameObject.Find("GUI Text");
}


function Update ()

{

 scoreText = "TotalScore: " + score;    
 
}

function OnTriggerEnter( other : Collider ) {

 if (other.tag == "coin") {
 
     score += 5;
 
     scoreText = "TotalScore: " + score;
     
     _label.guiText.text = scoreText;
     
     Destroy(other.gameObject);
 
 }
 
}

