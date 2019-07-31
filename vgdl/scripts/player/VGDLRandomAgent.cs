namespace vgdl.scripts.player
{
    public class VGDLRandomAgent : VGDLPlayer
    {
        public VGDLRandomAgent()
        {
            //fake human
            isHuman = false;
        }

        public override VGDLAvatarActions act(StateObservation stateObs, ElapsedCpuTimer elapsedTimer)
        {   
            return stateObs.getAvailableActions().RandomElement();
        }

        public override VGDLAvatarActions act(StateObservationMulti stateObs, ElapsedCpuTimer elapsedTimer)
        {
            return stateObs.getAvailableActions().RandomElement();
        }
    }
}