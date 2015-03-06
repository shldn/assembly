
public class CharacterLookAtLocalPlayer : CharacterLookAt {
    protected override void Start()
    {
        base.Start();
        UseCharacterOffset();
        if (lookAtGameObject == null && GroundGameManager.Inst.LocalPlayer != null)
            lookAtGameObject = GroundGameManager.Inst.LocalPlayer.gameObject;
    }	
}
