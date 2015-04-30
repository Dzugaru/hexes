module frontendMock;
import interfacing;
import enums;

extern(C) GrObjHandle createGrObjMock(GrObjClass objClass, GrObjType objType)
{
	return GrObjHandle(objClass, 0);
}

void setup()
{
	interfacing.cb.setBlackholes();
	interfacing.cb.createGrObj = &createGrObjMock;
}
