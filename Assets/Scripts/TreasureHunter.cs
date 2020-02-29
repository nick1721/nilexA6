using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TreasureHunter : MonoBehaviour
{
    
    public GameObject leftPointerObject;

    public GameObject rightPointerObject;

    public collectible[] collectiblesInScene;

    public TreasureHunterInventory inventory;

    public int score = 0;

    public TextMesh scoreSummary;

    public TextMesh itemSummary;

    public GameObject playerCamera;


    public collectible collectedItem;

    public int numOfItems = 0;

    public LayerMask collectiblesMask;

    collectible thingIGrabbed;

    Vector3 previousPointerPos;

    public enum AttachmentRule{KeepRelative, KeepWorld, SnapToTarget}

    public GameObject cameraRig;

    public GameObject playerController;

    bool launched = false;
    

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = playerCamera.transform.position;
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)){ 

            // "Force Grab Distance";
            forceGrab(true);
            
          
        } else if (OVRInput.GetDown(OVRInput.RawButton.A)){
            // "Grip";
            Collider[] overlappingThings=Physics.OverlapSphere(rightPointerObject.transform.position,0.05f,collectiblesMask);
            if (overlappingThings.Length>0){
                attachGameObjectToAChildGameObject(overlappingThings[0].gameObject,rightPointerObject,AttachmentRule.KeepWorld,AttachmentRule.KeepWorld,AttachmentRule.KeepWorld,true);
                //I'm not bothering to check for nullity because layer mask should ensure I only collect collectibles.
                thingIGrabbed=overlappingThings[0].gameObject.GetComponent<collectible>();
                thingIGrabbed.gameObject.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            }
        
        } else if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger) || OVRInput.GetUp(OVRInput.RawButton.B) ||OVRInput.GetUp(OVRInput.RawButton.RHandTrigger) || OVRInput.GetUp(OVRInput.RawButton.A)) {
            //"R Index Trigger Pressed Drop";
            
            letGo();

        } else if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)){
            // ### "Force Grab Snap";
            forceGrab(false);

        } else if (OVRInput.GetDown(OVRInput.RawButton.B)){
            // ### "Magnetic Grip";
            Collider[] overlappingThings=Physics.OverlapSphere(rightPointerObject.transform.position,1,collectiblesMask);
            if (overlappingThings.Length>0){
                collectible nearestCollectible=getClosestHitObject(overlappingThings);
                attachGameObjectToAChildGameObject(nearestCollectible.gameObject,rightPointerObject,AttachmentRule.SnapToTarget,AttachmentRule.SnapToTarget,AttachmentRule.KeepWorld,true);
                thingIGrabbed=nearestCollectible.gameObject.GetComponent<collectible>(); 
            }
        }
        previousPointerPos=rightPointerObject.gameObject.transform.position;


    } 

    collectible getClosestHitObject(Collider[] hits){ 
        float closestDistance=10000.0f;
        collectible closestObjectSoFar=null;
        foreach (Collider hit in hits){  // outHit??
            collectible c=hit.gameObject.GetComponent<collectible>();
            if (c){
                float distanceBetweenHandAndObject=(c.gameObject.transform.position-rightPointerObject.gameObject.transform.position).magnitude;
                if (distanceBetweenHandAndObject<closestDistance){
                    closestDistance=distanceBetweenHandAndObject;
                    closestObjectSoFar=c;
                }
            }
        }
        return closestObjectSoFar;
    }

    void forceGrab(bool pressedA){
    RaycastHit outHit;
    
    //notice I'm using the layer mask again
        if (Physics.Raycast(rightPointerObject.transform.position, rightPointerObject.transform.forward, out outHit, 100.0f,collectiblesMask))
            {
                AttachmentRule howToAttach=pressedA?AttachmentRule.KeepWorld:AttachmentRule.SnapToTarget;
                attachGameObjectToAChildGameObject(outHit.collider.gameObject,rightPointerObject.gameObject,howToAttach,howToAttach,AttachmentRule.KeepWorld,true);
                thingIGrabbed=outHit.collider.gameObject.GetComponent<collectible>();

            } 
         
    }

   
    void letGo(){
        Vector3 playerPosition = playerCamera.transform.position;
        if (thingIGrabbed){

            if (rightPointerObject.transform.position.y < (playerCamera.transform.position.y - 0.6) && rightPointerObject.transform.position.y > (playerCamera.transform.position.y - 1.2)) {                    
                GameObject objGrabbed = Resources.Load<GameObject>(thingIGrabbed.name);
                collectible currentCollectible = objGrabbed.gameObject.GetComponent<collectible>();

                if (currentCollectible.name =="Trap")
                {               
                    cameraRig.GetComponent<Rigidbody>().isKinematic = false;
                    scoreSummary.text = "YOU'VE BEEN CURSED!" + '\n' + "Get to the minivan to reverse the curse!";
                } 
                else if (currentCollectible.name == "Christ")
                {
                    cameraRig.GetComponent<Rigidbody>().isKinematic = true;
                }
                else {
                    if (inventory.itemsCollected.ContainsKey(currentCollectible)) 
                    {
                        inventory.itemsCollected[currentCollectible]++;
                    } 
                    else 
                    {
                        inventory.itemsCollected.Add(currentCollectible, 1);
                    }
                    score = score + currentCollectible.points;
                    numOfItems++;
                    scoreSummary.text = "Nick & Alex's Score: " + score + "\n" +
                                        "# of items: " + numOfItems;
                    itemSummary.text = " ";
                    foreach (KeyValuePair<collectible, int> item in inventory.itemsCollected) {
                        itemSummary.text += "\n # of " + item.Key.name + ": " + item.Value + ", Item Value: " + item.Key.points; 
                    }   
                }
                
                detachGameObject(thingIGrabbed.gameObject,AttachmentRule.KeepWorld,AttachmentRule.KeepWorld,AttachmentRule.KeepWorld);
                simulatePhysics(thingIGrabbed.gameObject,(rightPointerObject.gameObject.transform.position-previousPointerPos)/Time.deltaTime,true);
                Destroy(thingIGrabbed.gameObject);
                thingIGrabbed=null;               
            }else{
                detachGameObject(thingIGrabbed.gameObject,AttachmentRule.KeepWorld,AttachmentRule.KeepWorld,AttachmentRule.KeepWorld);
                simulatePhysics(thingIGrabbed.gameObject,(rightPointerObject.gameObject.transform.position-previousPointerPos)/Time.deltaTime,true);
                thingIGrabbed=null;
            }
        }
        
    }

    //public void spawnCage() {
        
        //Instantiate(cagePrefab, new Vector3(playerCamera.transform.position.x, playerCamera.transform.position.y + 2, playerCamera.transform.position.z), Quaternion.identity);
        //Instantiate(Resources.Load("Cage"), new Vector3(-36, 4, 12), Quaternion.identity);

    //}

    //since Unity doesn't have sceneComponents like UE4, we can only attach GOs to other GOs which are children of another GO
    //e.g. attach collectible to controller GO, which is a child of VRRoot GO
    //imagine if scenecomponents in UE4 were all split into distinct GOs in Unity
    public void attachGameObjectToAChildGameObject(GameObject GOToAttach, GameObject newParent, AttachmentRule locationRule, AttachmentRule rotationRule, AttachmentRule scaleRule, bool weld){
        GOToAttach.transform.parent=newParent.transform;
        handleAttachmentRules(GOToAttach,locationRule,rotationRule,scaleRule);
        if (weld){
            simulatePhysics(GOToAttach,Vector3.zero,false);
        }
    }

    public static void detachGameObject(GameObject GOToDetach, AttachmentRule locationRule, AttachmentRule rotationRule, AttachmentRule scaleRule){
        //making the parent null sets its parent to the world origin (meaning relative & global transforms become the same)
        GOToDetach.transform.parent=null;
        handleAttachmentRules(GOToDetach,locationRule,rotationRule,scaleRule);
    }

    public static void handleAttachmentRules(GameObject GOToHandle, AttachmentRule locationRule, AttachmentRule rotationRule, AttachmentRule scaleRule){
        GOToHandle.transform.localPosition=
        (locationRule==AttachmentRule.KeepRelative)?GOToHandle.transform.position:
        //technically don't need to change anything but I wanted to compress into ternary
        (locationRule==AttachmentRule.KeepWorld)?GOToHandle.transform.localPosition:
        new Vector3(0,0,0);

        //localRotation in Unity is actually a Quaternion, so we need to specifically ask for Euler angles
        GOToHandle.transform.localEulerAngles=
        (rotationRule==AttachmentRule.KeepRelative)?GOToHandle.transform.eulerAngles:
        //technically don't need to change anything but I wanted to compress into ternary
        (rotationRule==AttachmentRule.KeepWorld)?GOToHandle.transform.localEulerAngles:
        new Vector3(0,0,0);

        GOToHandle.transform.localScale=
        (scaleRule==AttachmentRule.KeepRelative)?GOToHandle.transform.lossyScale:
        //technically don't need to change anything but I wanted to compress into ternary
        (scaleRule==AttachmentRule.KeepWorld)?GOToHandle.transform.localScale:
        new Vector3(1,1,1);
    }
 
    public void simulatePhysics(GameObject target,Vector3 oldParentVelocity,bool simulate){
        Rigidbody rb=target.GetComponent<Rigidbody>();
        if (rb){
            if (!simulate){
                Destroy(rb);
            } 
        } else{
            if (simulate){
                //there's actually a problem here relative to the UE4 version since Unity doesn't have this simple "simulate physics" option
                //The object will NOT preserve momentum when you throw it like in UE4.
                //need to set its velocity itself.... even if you switch the kinematic/gravity settings around instead of deleting/adding rb
                Rigidbody newRB=target.AddComponent<Rigidbody>();
                newRB.velocity=oldParentVelocity;
            }
        }  
            
    }  
}

