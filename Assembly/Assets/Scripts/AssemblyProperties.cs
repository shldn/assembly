using System;

[Serializable]
public class AssemblyProperties {

    // Only used to make sure the Model and Viewer have different copies, once the split is made, this won't be neccessary.
    public AssemblyProperties(AssemblyProperties ap) {
        id = ap.id;
        matingWith = ap.matingWith;
        name = ap.name;
        gender = ap.gender;
        wantToMate = ap.wantToMate;
    }
    public AssemblyProperties() {
        id = -1;
        matingWith = -1;
        name = "NoName";
        gender = false;
        wantToMate = false;
    }

    public int id = -1;
    public int matingWith = -1;
    public string name = "NoName";
    public bool gender = false;
    public bool wantToMate = false;
}
