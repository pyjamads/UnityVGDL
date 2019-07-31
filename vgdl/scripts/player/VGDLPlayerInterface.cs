
public interface VGDLPlayerInterface 
{
    int PlayerID { get; set; }
    VGDLAvatarActions lastAction { get; }
    bool isHuman { get; }
    VGDLAvatarActions act(StateObservation stateObs, ElapsedCpuTimer elapsedTimer);
    VGDLAvatarActions act(StateObservationMulti stateObs, ElapsedCpuTimer elapsedTimer);
    void result(StateObservation stateObs, ElapsedCpuTimer elapsedCpuTimer);
    void result(StateObservationMulti stateObs, ElapsedCpuTimer elapsedCpuTimer);
    void setup(string actionFile, int randomSeed, bool isHuman);
    void teardown(VGDLGame played);
    void logAction(VGDLAvatarActions action);
}